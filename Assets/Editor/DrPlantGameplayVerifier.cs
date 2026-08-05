using System;
using System.Collections.Generic;
using DrPlant.Data;
using DrPlant.Gameplay;
using UnityEditor;
using UnityEngine;

public static class DrPlantGameplayVerifier
{
    [MenuItem("Dr.Plant/Verify/Core Gameplay Rules")]
    public static void VerifyCoreGameplay()
    {
        DrPlantContentCatalog catalog =
            AssetDatabase.LoadAssetAtPath<DrPlantContentCatalog>(
                "Assets/Resources/DrPlantContentCatalog.asset");

        Require(catalog != null, "Content catalog could not be loaded.");
        Require(catalog.TryGetPatient(PatientId.Flower, out PatientDefinition patient),
            "Flower patient is missing.");
        Require(patient.Voice != null, "Flower patient voice is missing.");
        Require(
            Mathf.Approximately(patient.Voice.Frequency, 650f)
            && patient.Voice.Waveform == PatientVoiceWaveform.Sine
            && Mathf.Approximately(patient.Voice.PitchVariation, 45f),
            "Flower patient voice does not match the source data.");
        Require(catalog.TryGetSymptom(
            SymptomId.Dehydration,
            out SymptomDefinition dehydration),
            "Dehydration symptom is missing.");
        Require(catalog.TryGetSymptom(SymptomId.Hot, out SymptomDefinition hot),
            "Hot symptom is missing.");

        PatientCase single = new PatientCase(
            patient,
            new[] { dehydration });
        Require(
            TreatmentEvaluator.IsExactMatch(single, new[] { TreatmentId.Water }),
            "Water must treat dehydration.");
        Require(
            !TreatmentEvaluator.IsExactMatch(single, new[] { TreatmentId.Cool }),
            "Cooling must not treat dehydration.");
        Require(
            !TreatmentEvaluator.IsExactMatch(single, Array.Empty<TreatmentId>()),
            "An empty selection must fail.");

        PatientCase composite = new PatientCase(
            patient,
            new[] { dehydration, hot });
        Require(
            TreatmentEvaluator.IsExactMatch(
                composite,
                new[] { TreatmentId.Water, TreatmentId.Cool }),
            "Water and cooling must treat dehydration plus heat.");
        Require(
            TreatmentEvaluator.IsExactMatch(
                composite,
                new[] { TreatmentId.Water, TreatmentId.Sunglasses }),
            "Unlocked sunglasses must be a valid alternative for heat.");
        Require(
            !TreatmentEvaluator.IsExactMatch(
                composite,
                new[] { TreatmentId.Water }),
            "Missing one treatment must fail.");
        Require(
            !TreatmentEvaluator.IsExactMatch(
                composite,
                new[] { TreatmentId.Water, TreatmentId.Cool, TreatmentId.Warm }),
            "Extra treatments must fail.");

        HashSet<ShopItemId> sunglassesOwned = new HashSet<ShopItemId>
        {
            ShopItemId.Sunglasses
        };
        Require(
            !catalog.IsTreatmentUnlocked(TreatmentId.Sunglasses, null),
            "Sunglasses must start locked.");
        Require(
            catalog.IsTreatmentUnlocked(TreatmentId.Sunglasses, sunglassesOwned),
            "Purchased sunglasses must unlock their treatment.");

        for (int index = 0; index < 250; index++)
        {
            PatientCase generated = PatientCaseGenerator.Create(catalog);

            Require(generated.Patient.IsPlayable, "Generator selected a patient without a prefab.");
            Require(
                generated.Symptoms.Count >= 1
                && generated.Symptoms.Count <= catalog.Rules.MaxSymptoms,
                "Generator produced an invalid symptom count.");
            Require(
                !generated.HasSymptom(SymptomId.Boredom)
                && !generated.HasSymptom(SymptomId.Overgrown),
                "Locked symptoms appeared before their shop purchases.");
            Require(
                !(generated.HasSymptom(SymptomId.Hot)
                  && generated.HasSymptom(SymptomId.Cold)),
                "Hot and cold appeared together.");
        }

        for (int index = 0; index < 100; index++)
        {
            int goodReward = TreatmentEvaluator.RollReward(catalog.Rules, true);
            int badReward = TreatmentEvaluator.RollReward(catalog.Rules, false);

            Require(
                goodReward >= catalog.Rules.CorrectRewardMin
                && goodReward <= catalog.Rules.CorrectRewardMax,
                "Correct reward was outside its configured range.");
            Require(
                badReward >= catalog.Rules.IncorrectRewardMin
                && badReward <= catalog.Rules.IncorrectRewardMax,
                "Incorrect reward was outside its configured range.");
        }

        Debug.Log("Dr.Plant core gameplay verification passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
