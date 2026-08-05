using System;
using System.Collections;
using System.Collections.Generic;
using DrPlant.Data;
using DrPlant.Gameplay;
using DrPlant.Progression;
using UnityEngine;

public sealed class DrPlantRuntimeSmokeVerifier : MonoBehaviour
{
    private const string CommandLineFlag = "-drplant-smoke-test";
    private const float LoadTimeoutSeconds = 5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartWhenRequested()
    {
        string[] arguments = Environment.GetCommandLineArgs();

        for (int index = 0; index < arguments.Length; index++)
        {
            if (!string.Equals(
                arguments[index],
                CommandLineFlag,
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            GameObject verifierObject = new GameObject(nameof(DrPlantRuntimeSmokeVerifier));
            DontDestroyOnLoad(verifierObject);
            verifierObject.AddComponent<DrPlantRuntimeSmokeVerifier>();
            return;
        }
    }

    private IEnumerator Start()
    {
        float deadline = Time.realtimeSinceStartup + LoadTimeoutSeconds;

        while (!IsClinicLoaded() && Time.realtimeSinceStartup < deadline)
            yield return null;

        if (!Check(IsClinicLoaded(), "The clinic scene did not initialize."))
            yield break;

        PatientManager patientManager = PatientManager.Instance;
        ChecklistManager checklistManager = ChecklistManager.Instance;
        MoneyManager moneyManager = MoneyManager.Instance;
        ShopManager shopManager = ShopManager.Instance;
        DrPlantContentCatalog catalog = DrPlantContent.Catalog;
        ClinicProgress progress = ClinicProgress.Instance;

        progress.Reset();
        yield return null;

        if (!patientManager.PatientReady)
        {
            if (!Check(
                CompleteMove(patientManager.CurrentPatient),
                "The first patient has no arrival callback."))
            {
                yield break;
            }

            yield return null;
        }

        if (!Check(patientManager.PatientReady, "The first patient did not become ready."))
            yield break;

        if (!Check(checklistManager.ItemCount == 4,
            $"Expected 4 base treatments, found {checklistManager.ItemCount}."))
        {
            yield break;
        }

        if (!Check(shopManager.ItemCount == catalog.ShopItems.Count,
            $"Expected {catalog.ShopItems.Count} shop items, found {shopManager.ItemCount}."))
        {
            yield break;
        }

        if (!Check(
            shopManager.TryPurchase(ShopItemId.Instrument)
                == PurchaseResult.InsufficientFunds,
            "A purchase succeeded without enough money."))
        {
            yield break;
        }

        int totalPrice = 0;
        foreach (ShopItemDefinition item in catalog.ShopItems)
            totalPrice += item.Price;

        progress.AddMoney(totalPrice);

        for (int index = 0; index < catalog.ShopItems.Count; index++)
        {
            ShopItemDefinition item = catalog.ShopItems[index];

            if (!Check(
                shopManager.TryPurchase(item.Id) == PurchaseResult.Success,
                $"Could not purchase {item.Id}."))
            {
                yield break;
            }

            if (!Check(
                checklistManager.ItemCount == 5 + index,
                $"Checklist did not unlock treatment {item.UnlockedTreatment}."))
            {
                yield break;
            }
        }

        ShopItemDefinition firstItem = catalog.ShopItems[0];
        int moneyAfterPurchases = progress.Money;

        if (!Check(
            shopManager.TryPurchase(firstItem.Id) == PurchaseResult.AlreadyPurchased,
            "A duplicate purchase was not rejected."))
        {
            yield break;
        }

        if (!Check(
            progress.Money == moneyAfterPurchases,
            "A duplicate purchase changed the saved money."))
        {
            yield break;
        }

        if (!Check(
            progress.PurchasedItemCount == catalog.ShopItems.Count,
            "Not all purchased items were recorded."))
        {
            yield break;
        }

        HashSet<ShopItemId> purchasedItems = progress.GetPurchasedShopItems();

        if (!Check(
            catalog.IsSymptomUnlocked(SymptomId.Boredom, purchasedItems)
            && catalog.IsSymptomUnlocked(SymptomId.Overgrown, purchasedItems),
            "Purchased tools did not unlock advanced symptoms."))
        {
            yield break;
        }

        if (!Check(
            VerifyUnlockedCases(catalog, purchasedItems),
            "Generated cases did not use the purchased symptom unlocks."))
        {
            yield break;
        }

        progress.Reload();

        if (!Check(
            progress.PurchasedItemCount == catalog.ShopItems.Count
            && checklistManager.ItemCount == 7,
            "Purchased items did not survive a save reload."))
        {
            yield break;
        }

        PatientCase firstCase = patientManager.CurrentCase;
        GameObject firstPatient = patientManager.CurrentPatient;
        HashSet<TreatmentId> correctTreatments = BuildCorrectTreatments(
            firstCase,
            catalog,
            progress.GetPurchasedShopItems());

        if (!Check(correctTreatments.Count == firstCase.Symptoms.Count,
            "Could not choose one unlocked treatment for each symptom."))
        {
            yield break;
        }

        checklistManager.SetSelectedTreatments(correctTreatments);
        int moneyBefore = moneyManager.money;
        int treatedBefore = progress.TreatedPatientCount;

        patientManager.SendPatient();

        int reward = moneyManager.money - moneyBefore;
        if (!Check(
            reward >= catalog.Rules.CorrectRewardMin
            && reward <= catalog.Rules.CorrectRewardMax,
            $"Correct treatment reward was outside the configured range: {reward}."))
        {
            yield break;
        }

        if (!Check(
            progress.TreatedPatientCount == treatedBefore + 1,
            "Treated patient progress was not recorded."))
        {
            yield break;
        }

        int savedMoney = progress.Money;
        int savedTreatedCount = progress.TreatedPatientCount;
        progress.Reload();

        if (!Check(
            progress.Money == savedMoney
            && progress.TreatedPatientCount == savedTreatedCount,
            "Money or treated patient count did not survive a save reload."))
        {
            yield break;
        }

        if (!Check(CompleteMove(firstPatient), "The treated patient has no exit callback."))
            yield break;

        yield return null;

        GameObject nextPatient = patientManager.CurrentPatient;
        if (!Check(
            nextPatient != null && nextPatient != firstPatient,
            "The next patient was not spawned."))
        {
            yield break;
        }

        if (!patientManager.PatientReady)
        {
            if (!Check(CompleteMove(nextPatient), "The next patient has no arrival callback."))
                yield break;

            yield return null;
        }

        if (!Check(patientManager.PatientReady, "The next patient did not become ready."))
            yield break;

        progress.DeleteSave();
        Debug.Log("Dr.Plant progression, shop, and save smoke test passed.");
        Application.Quit(0);
    }

    private static bool IsClinicLoaded()
    {
        return PatientManager.Instance != null
            && PatientManager.Instance.CurrentPatient != null
            && PatientManager.Instance.CurrentCase != null
            && ChecklistManager.Instance != null
            && MoneyManager.Instance != null
            && ShopManager.Instance != null
            && ShopManager.Instance.ItemCount > 0
            && DrPlantContent.Catalog != null;
    }

    private static bool CompleteMove(GameObject patient)
    {
        if (patient == null)
            return false;

        PatientMove move = patient.GetComponent<PatientMove>();
        Action onArrive = move != null ? move.onArrive : null;

        if (onArrive == null)
            return false;

        onArrive.Invoke();
        return true;
    }

    private static bool VerifyUnlockedCases(
        DrPlantContentCatalog catalog,
        ISet<ShopItemId> purchasedItems)
    {
        bool foundBoredom = false;
        bool foundOvergrown = false;

        for (int index = 0; index < 250; index++)
        {
            PatientCase generated = PatientCaseGenerator.Create(catalog, purchasedItems);
            foundBoredom |= generated.HasSymptom(SymptomId.Boredom);
            foundOvergrown |= generated.HasSymptom(SymptomId.Overgrown);

            if (generated.HasSymptom(SymptomId.Hot)
                && generated.HasSymptom(SymptomId.Cold))
            {
                return false;
            }
        }

        return foundBoredom && foundOvergrown;
    }

    private static HashSet<TreatmentId> BuildCorrectTreatments(
        PatientCase patientCase,
        DrPlantContentCatalog catalog,
        ISet<ShopItemId> purchasedItems)
    {
        HashSet<TreatmentId> selected = new HashSet<TreatmentId>();

        foreach (SymptomDefinition symptom in patientCase.Symptoms)
        {
            foreach (TreatmentId treatmentId in symptom.AcceptedTreatments)
            {
                if (!selected.Contains(treatmentId)
                    && catalog.IsTreatmentUnlocked(treatmentId, purchasedItems))
                {
                    selected.Add(treatmentId);
                    break;
                }
            }
        }

        return selected;
    }

    private static bool Check(bool condition, string message)
    {
        if (condition)
            return true;

        Debug.LogError($"Dr.Plant runtime smoke test failed: {message}");
        Application.Quit(1);
        return false;
    }
}
