using System;
using System.Collections.Generic;
using DrPlant.Data;
using DrPlant.Progression;
using UnityEditor;
using UnityEngine;

public static class DrPlantProgressionVerifier
{
    [MenuItem("Dr.Plant/Verify/Progression And Save")]
    public static void VerifyProgressionAndSave()
    {
        DrPlantContentCatalog catalog =
            AssetDatabase.LoadAssetAtPath<DrPlantContentCatalog>(
                "Assets/Resources/DrPlantContentCatalog.asset");

        Require(catalog != null, "Content catalog could not be loaded.");
        Require(catalog.ShopItems.Count == 3, "Expected three shop items.");

        string testKey = $"DrPlant.Progression.EditorTest.{Guid.NewGuid():N}";
        ClinicProgress progress = new ClinicProgress(testKey);

        try
        {
            progress.DeleteSave();

            Require(
                catalog.TryGetShopItem(
                    ShopItemId.Instrument,
                    out ShopItemDefinition instrument),
                "Instrument shop item is missing.");
            Require(
                catalog.TryGetShopItem(
                    ShopItemId.Scissors,
                    out ShopItemDefinition scissors),
                "Scissors shop item is missing.");
            Require(
                catalog.TryGetShopItem(
                    ShopItemId.Sunglasses,
                    out ShopItemDefinition sunglasses),
                "Sunglasses shop item is missing.");

            progress.AddMoney(instrument.Price - 1);
            Require(
                progress.TryPurchase(instrument) == PurchaseResult.InsufficientFunds,
                "A purchase must fail when one gold short.");
            Require(
                progress.Money == instrument.Price - 1,
                "A failed purchase changed the balance.");

            progress.AddMoney(1);
            Require(
                progress.TryPurchase(instrument) == PurchaseResult.Success,
                "Instrument purchase failed.");
            Require(progress.Money == 0, "Instrument price was not deducted exactly.");
            Require(progress.IsPurchased(ShopItemId.Instrument),
                "Instrument ownership was not recorded.");

            Require(
                progress.TryPurchase(instrument) == PurchaseResult.AlreadyPurchased,
                "Duplicate instrument purchase was not rejected.");
            Require(progress.Money == 0, "Duplicate purchase changed the balance.");

            progress.Reload();
            Require(progress.IsPurchased(ShopItemId.Instrument),
                "Instrument ownership was not loaded from storage.");

            progress.AddMoney(scissors.Price + sunglasses.Price);
            Require(
                progress.TryPurchase(scissors) == PurchaseResult.Success,
                "Scissors purchase failed.");
            Require(
                progress.TryPurchase(sunglasses) == PurchaseResult.Success,
                "Sunglasses purchase failed.");
            Require(progress.Money == 0, "Shop prices were not deducted exactly.");
            Require(progress.PurchasedItemCount == 3,
                "All purchased items were not recorded.");

            HashSet<ShopItemId> purchased = progress.GetPurchasedShopItems();
            Require(
                catalog.IsTreatmentUnlocked(TreatmentId.Music, purchased),
                "Instrument did not unlock music.");
            Require(
                catalog.IsTreatmentUnlocked(TreatmentId.Prune, purchased),
                "Scissors did not unlock pruning.");
            Require(
                catalog.IsTreatmentUnlocked(TreatmentId.Sunglasses, purchased),
                "Sunglasses did not unlock their treatment.");
            Require(
                catalog.IsSymptomUnlocked(SymptomId.Boredom, purchased),
                "Instrument did not unlock boredom.");
            Require(
                catalog.IsSymptomUnlocked(SymptomId.Overgrown, purchased),
                "Scissors did not unlock overgrowth.");

            progress.CompleteTreatment(175);
            Require(progress.Money == 175, "Treatment reward was not recorded.");
            Require(progress.TreatedPatientCount == 1,
                "Treated patient count was not recorded.");

            progress.Reload();
            Require(progress.Money == 175, "Money was not restored after reload.");
            Require(progress.TreatedPatientCount == 1,
                "Treated patient count was not restored after reload.");
            Require(progress.PurchasedItemCount == 3,
                "Purchases were not restored after reload.");

            progress.Reset();
            Require(
                progress.Money == 0
                && progress.TreatedPatientCount == 0
                && progress.PurchasedItemCount == 0,
                "Reset did not clear progress.");
        }
        finally
        {
            progress.DeleteSave();
        }

        Debug.Log("Dr.Plant progression and save verification passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
