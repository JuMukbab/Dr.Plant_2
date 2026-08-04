using System;
using System.Collections;
using System.Collections.Generic;
using DrPlant.Data;
using DrPlant.Gameplay;
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
        DrPlantContentCatalog catalog = DrPlantContent.Catalog;

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

        PatientCase firstCase = patientManager.CurrentCase;
        GameObject firstPatient = patientManager.CurrentPatient;
        HashSet<TreatmentId> correctTreatments = BuildCorrectTreatments(firstCase, catalog);

        if (!Check(correctTreatments.Count == firstCase.Symptoms.Count,
            "Could not choose one unlocked treatment for each symptom."))
        {
            yield break;
        }

        checklistManager.SetSelectedTreatments(correctTreatments);
        int moneyBefore = moneyManager.money;

        patientManager.SendPatient();

        int reward = moneyManager.money - moneyBefore;
        if (!Check(
            reward >= catalog.Rules.CorrectRewardMin
            && reward <= catalog.Rules.CorrectRewardMax,
            $"Correct treatment reward was outside the configured range: {reward}."))
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

        Debug.Log("Dr.Plant runtime core loop smoke test passed.");
        Application.Quit(0);
    }

    private static bool IsClinicLoaded()
    {
        return PatientManager.Instance != null
            && PatientManager.Instance.CurrentPatient != null
            && PatientManager.Instance.CurrentCase != null
            && ChecklistManager.Instance != null
            && MoneyManager.Instance != null
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

    private static HashSet<TreatmentId> BuildCorrectTreatments(
        PatientCase patientCase,
        DrPlantContentCatalog catalog)
    {
        HashSet<TreatmentId> selected = new HashSet<TreatmentId>();

        foreach (SymptomDefinition symptom in patientCase.Symptoms)
        {
            foreach (TreatmentId treatmentId in symptom.AcceptedTreatments)
            {
                if (!selected.Contains(treatmentId)
                    && catalog.IsTreatmentUnlocked(treatmentId, null))
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
