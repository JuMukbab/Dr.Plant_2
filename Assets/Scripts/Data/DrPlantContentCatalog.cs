using System;
using System.Collections.Generic;
using UnityEngine;

namespace DrPlant.Data
{
    [CreateAssetMenu(
        fileName = "DrPlantContentCatalog",
        menuName = "Dr.Plant/Content Catalog")]
    public sealed class DrPlantContentCatalog : ScriptableObject
    {
        [SerializeField] private ClinicRules rules;
        [SerializeField] private PatientDefinition[] patients = Array.Empty<PatientDefinition>();
        [SerializeField] private SymptomDefinition[] symptoms = Array.Empty<SymptomDefinition>();
        [SerializeField] private TreatmentDefinition[] treatments = Array.Empty<TreatmentDefinition>();
        [SerializeField] private ShopItemDefinition[] shopItems = Array.Empty<ShopItemDefinition>();
        [SerializeField] private DialogueLibrary dialogues;

        public ClinicRules Rules => rules;
        public IReadOnlyList<PatientDefinition> Patients => patients;
        public IReadOnlyList<SymptomDefinition> Symptoms => symptoms;
        public IReadOnlyList<TreatmentDefinition> Treatments => treatments;
        public IReadOnlyList<ShopItemDefinition> ShopItems => shopItems;
        public DialogueLibrary Dialogues => dialogues;

        public bool TryGetPatient(PatientId id, out PatientDefinition definition)
        {
            for (int index = 0; index < patients.Length; index++)
            {
                if (patients[index] != null && patients[index].Id == id)
                {
                    definition = patients[index];
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public bool TryGetSymptom(SymptomId id, out SymptomDefinition definition)
        {
            for (int index = 0; index < symptoms.Length; index++)
            {
                if (symptoms[index] != null && symptoms[index].Id == id)
                {
                    definition = symptoms[index];
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public bool TryGetTreatment(TreatmentId id, out TreatmentDefinition definition)
        {
            for (int index = 0; index < treatments.Length; index++)
            {
                if (treatments[index] != null && treatments[index].Id == id)
                {
                    definition = treatments[index];
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public bool TryGetShopItem(ShopItemId id, out ShopItemDefinition definition)
        {
            for (int index = 0; index < shopItems.Length; index++)
            {
                if (shopItems[index] != null && shopItems[index].Id == id)
                {
                    definition = shopItems[index];
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public bool IsTreatmentUnlocked(
            TreatmentId id,
            ISet<ShopItemId> purchasedShopItems)
        {
            return TryGetTreatment(id, out TreatmentDefinition treatment)
                && HasRequiredItem(treatment.RequiredShopItem, purchasedShopItems);
        }

        public bool IsSymptomUnlocked(
            SymptomId id,
            ISet<ShopItemId> purchasedShopItems)
        {
            return TryGetSymptom(id, out SymptomDefinition symptom)
                && HasRequiredItem(symptom.RequiredShopItem, purchasedShopItems);
        }

        public List<string> GetValidationErrors()
        {
            List<string> errors = new List<string>();

            ValidateRules(errors);
            ValidatePatients(errors);
            ValidateTreatments(errors);
            ValidateShopItems(errors);
            ValidateSymptoms(errors);
            ValidateDialogues(errors);

            return errors;
        }

        private static bool HasRequiredItem(
            ShopItemId requiredItem,
            ISet<ShopItemId> purchasedShopItems)
        {
            return requiredItem == ShopItemId.None
                || purchasedShopItems != null && purchasedShopItems.Contains(requiredItem);
        }

        private void ValidateRules(List<string> errors)
        {
            if (rules == null)
            {
                errors.Add("Clinic rules are missing.");
                return;
            }

            if (rules.CompositeSymptomChance < 0f || rules.CompositeSymptomChance > 1f)
                errors.Add("Composite symptom chance must be between 0 and 1.");

            if (rules.NormalDialogueChance < 0f || rules.NormalDialogueChance > 1f)
                errors.Add("Normal dialogue chance must be between 0 and 1.");

            if (rules.MaxSymptoms < 1)
                errors.Add("Max symptoms must be at least 1.");

            if (rules.CorrectRewardMin > rules.CorrectRewardMax)
                errors.Add("Correct reward minimum cannot exceed its maximum.");

            if (rules.IncorrectRewardMin > rules.IncorrectRewardMax)
                errors.Add("Incorrect reward minimum cannot exceed its maximum.");
        }

        private void ValidatePatients(List<string> errors)
        {
            HashSet<PatientId> ids = new HashSet<PatientId>();

            foreach (PatientDefinition patient in patients)
            {
                if (patient == null)
                {
                    errors.Add("Patient definition cannot be null.");
                    continue;
                }

                if (patient.Id == PatientId.None || !ids.Add(patient.Id))
                    errors.Add($"Patient ID is missing or duplicated: {patient.Id}.");

                if (string.IsNullOrWhiteSpace(patient.DisplayName))
                    errors.Add($"Patient {patient.Id} needs a display name.");
            }
        }

        private void ValidateTreatments(List<string> errors)
        {
            HashSet<TreatmentId> ids = new HashSet<TreatmentId>();

            foreach (TreatmentDefinition treatment in treatments)
            {
                if (treatment == null)
                {
                    errors.Add("Treatment definition cannot be null.");
                    continue;
                }

                if (treatment.Id == TreatmentId.None || !ids.Add(treatment.Id))
                    errors.Add($"Treatment ID is missing or duplicated: {treatment.Id}.");

                if (string.IsNullOrWhiteSpace(treatment.DisplayName))
                    errors.Add($"Treatment {treatment.Id} needs a display name.");

                if (treatment.RequiredShopItem != ShopItemId.None
                    && !TryGetShopItem(treatment.RequiredShopItem, out _))
                {
                    errors.Add(
                        $"Treatment {treatment.Id} requires an unknown shop item: "
                        + $"{treatment.RequiredShopItem}.");
                }
            }
        }

        private void ValidateShopItems(List<string> errors)
        {
            HashSet<ShopItemId> ids = new HashSet<ShopItemId>();

            foreach (ShopItemDefinition item in shopItems)
            {
                if (item == null)
                {
                    errors.Add("Shop item definition cannot be null.");
                    continue;
                }

                if (item.Id == ShopItemId.None || !ids.Add(item.Id))
                    errors.Add($"Shop item ID is missing or duplicated: {item.Id}.");

                if (string.IsNullOrWhiteSpace(item.DisplayName))
                    errors.Add($"Shop item {item.Id} needs a display name.");

                if (item.Price < 0)
                    errors.Add($"Shop item {item.Id} cannot have a negative price.");

                if (!TryGetTreatment(item.UnlockedTreatment, out TreatmentDefinition treatment))
                {
                    errors.Add(
                        $"Shop item {item.Id} unlocks an unknown treatment: "
                        + $"{item.UnlockedTreatment}.");
                    continue;
                }

                if (treatment.RequiredShopItem != item.Id)
                {
                    errors.Add(
                        $"Shop item {item.Id} and treatment {treatment.Id} "
                        + "must reference each other.");
                }
            }
        }

        private void ValidateSymptoms(List<string> errors)
        {
            HashSet<SymptomId> ids = new HashSet<SymptomId>();

            foreach (SymptomDefinition symptom in symptoms)
            {
                if (symptom == null)
                {
                    errors.Add("Symptom definition cannot be null.");
                    continue;
                }

                if (symptom.Id == SymptomId.None || !ids.Add(symptom.Id))
                    errors.Add($"Symptom ID is missing or duplicated: {symptom.Id}.");

                if (string.IsNullOrWhiteSpace(symptom.DisplayName))
                    errors.Add($"Symptom {symptom.Id} needs a display name.");

                if (symptom.Dialogues.Count == 0)
                    errors.Add($"Symptom {symptom.Id} needs at least one dialogue.");

                if (symptom.RequiredShopItem != ShopItemId.None
                    && !TryGetShopItem(symptom.RequiredShopItem, out _))
                {
                    errors.Add(
                        $"Symptom {symptom.Id} requires an unknown shop item: "
                        + $"{symptom.RequiredShopItem}.");
                }

                HashSet<TreatmentId> accepted = new HashSet<TreatmentId>();
                foreach (TreatmentId treatmentId in symptom.AcceptedTreatments)
                {
                    if (!accepted.Add(treatmentId))
                        errors.Add($"Symptom {symptom.Id} repeats treatment {treatmentId}.");

                    if (!TryGetTreatment(treatmentId, out _))
                        errors.Add($"Symptom {symptom.Id} references unknown treatment {treatmentId}.");
                }

                if (accepted.Count == 0)
                    errors.Add($"Symptom {symptom.Id} needs an accepted treatment.");

                foreach (SymptomId incompatibleId in symptom.IncompatibleSymptoms)
                {
                    if (!TryGetSymptom(incompatibleId, out SymptomDefinition incompatible))
                    {
                        errors.Add(
                            $"Symptom {symptom.Id} references unknown incompatible symptom "
                            + $"{incompatibleId}.");
                        continue;
                    }

                    if (!Contains(incompatible.IncompatibleSymptoms, symptom.Id))
                    {
                        errors.Add(
                            $"Incompatibility between {symptom.Id} and {incompatibleId} "
                            + "must be declared on both symptoms.");
                    }
                }
            }
        }

        private void ValidateDialogues(List<string> errors)
        {
            if (dialogues == null)
            {
                errors.Add("Dialogue library is missing.");
                return;
            }

            if (dialogues.Arrival.Count == 0)
                errors.Add("Arrival dialogue list cannot be empty.");

            if (dialogues.Normal.Count == 0)
                errors.Add("Normal dialogue list cannot be empty.");

            if (dialogues.GoodReviews.Count == 0)
                errors.Add("Good review list cannot be empty.");

            if (dialogues.BadReviews.Count == 0)
                errors.Add("Bad review list cannot be empty.");
        }

        private static bool Contains(IReadOnlyList<SymptomId> values, SymptomId target)
        {
            for (int index = 0; index < values.Count; index++)
            {
                if (values[index] == target)
                    return true;
            }

            return false;
        }

#if UNITY_EDITOR
        public void ConfigureForEditor(
            ClinicRules rules,
            PatientDefinition[] patients,
            SymptomDefinition[] symptoms,
            TreatmentDefinition[] treatments,
            ShopItemDefinition[] shopItems,
            DialogueLibrary dialogues)
        {
            this.rules = rules;
            this.patients = patients ?? Array.Empty<PatientDefinition>();
            this.symptoms = symptoms ?? Array.Empty<SymptomDefinition>();
            this.treatments = treatments ?? Array.Empty<TreatmentDefinition>();
            this.shopItems = shopItems ?? Array.Empty<ShopItemDefinition>();
            this.dialogues = dialogues;
        }
#endif
    }
}
