using System;
using System.Collections.Generic;
using DrPlant.Data;
using UnityEngine;

namespace DrPlant.Gameplay
{
    public static class PatientCaseGenerator
    {
        public static PatientCase Create(
            DrPlantContentCatalog catalog,
            ISet<ShopItemId> purchasedShopItems = null)
        {
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            List<PatientDefinition> playablePatients = new List<PatientDefinition>();
            foreach (PatientDefinition patient in catalog.Patients)
            {
                if (patient != null && patient.IsPlayable)
                    playablePatients.Add(patient);
            }

            if (playablePatients.Count == 0)
                throw new InvalidOperationException("The content catalog has no playable patients.");

            List<SymptomDefinition> availableSymptoms = new List<SymptomDefinition>();
            foreach (SymptomDefinition symptom in catalog.Symptoms)
            {
                if (symptom != null
                    && catalog.IsSymptomUnlocked(symptom.Id, purchasedShopItems))
                {
                    availableSymptoms.Add(symptom);
                }
            }

            if (availableSymptoms.Count == 0)
                throw new InvalidOperationException("The content catalog has no unlocked symptoms.");

            PatientDefinition selectedPatient = Pick(playablePatients);
            List<SymptomDefinition> selectedSymptoms = new List<SymptomDefinition>
            {
                Pick(availableSymptoms)
            };

            ClinicRules rules = catalog.Rules;
            bool createComposite = rules.MaxSymptoms > 1
                && availableSymptoms.Count > 1
                && UnityEngine.Random.value < rules.CompositeSymptomChance;

            if (createComposite)
            {
                AddCompatibleSymptoms(
                    selectedSymptoms,
                    availableSymptoms,
                    Mathf.Min(rules.MaxSymptoms, availableSymptoms.Count));
            }

            return new PatientCase(selectedPatient, selectedSymptoms);
        }

        private static void AddCompatibleSymptoms(
            List<SymptomDefinition> selected,
            List<SymptomDefinition> available,
            int targetCount)
        {
            List<SymptomDefinition> candidates = new List<SymptomDefinition>(available);

            while (selected.Count < targetCount && candidates.Count > 0)
            {
                int candidateIndex = UnityEngine.Random.Range(0, candidates.Count);
                SymptomDefinition candidate = candidates[candidateIndex];
                candidates.RemoveAt(candidateIndex);

                if (selected.Contains(candidate) || IsIncompatibleWithAny(candidate, selected))
                    continue;

                selected.Add(candidate);
            }
        }

        private static bool IsIncompatibleWithAny(
            SymptomDefinition candidate,
            IReadOnlyList<SymptomDefinition> selected)
        {
            for (int index = 0; index < selected.Count; index++)
            {
                if (Contains(candidate.IncompatibleSymptoms, selected[index].Id)
                    || Contains(selected[index].IncompatibleSymptoms, candidate.Id))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Contains(
            IReadOnlyList<SymptomId> symptomIds,
            SymptomId target)
        {
            for (int index = 0; index < symptomIds.Count; index++)
            {
                if (symptomIds[index] == target)
                    return true;
            }

            return false;
        }

        private static T Pick<T>(IReadOnlyList<T> values)
        {
            return values[UnityEngine.Random.Range(0, values.Count)];
        }
    }
}
