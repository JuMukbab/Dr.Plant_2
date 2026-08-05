using System;
using System.Collections.Generic;
using DrPlant.Data;
using UnityEngine;

namespace DrPlant.Gameplay
{
    public readonly struct TreatmentOutcome
    {
        public bool IsCorrect { get; }
        public int Reward { get; }

        public TreatmentOutcome(bool isCorrect, int reward)
        {
            IsCorrect = isCorrect;
            Reward = reward;
        }
    }

    public static class TreatmentEvaluator
    {
        public static TreatmentOutcome Evaluate(
            PatientCase patientCase,
            IReadOnlyList<TreatmentId> selectedTreatments,
            ClinicRules rules)
        {
            if (patientCase == null)
                throw new ArgumentNullException(nameof(patientCase));

            if (rules == null)
                throw new ArgumentNullException(nameof(rules));

            bool isCorrect = IsExactMatch(patientCase, selectedTreatments);
            return new TreatmentOutcome(isCorrect, RollReward(rules, isCorrect));
        }

        public static bool IsExactMatch(
            PatientCase patientCase,
            IReadOnlyList<TreatmentId> selectedTreatments)
        {
            if (patientCase == null || selectedTreatments == null)
                return false;

            if (selectedTreatments.Count != patientCase.Symptoms.Count)
                return false;

            bool[] usedTreatments = new bool[selectedTreatments.Count];
            return CanMatchSymptoms(
                patientCase.Symptoms,
                selectedTreatments,
                symptomIndex: 0,
                usedTreatments);
        }

        public static int RollReward(ClinicRules rules, bool isCorrect)
        {
            int minimum = isCorrect
                ? rules.CorrectRewardMin
                : rules.IncorrectRewardMin;
            int maximum = isCorrect
                ? rules.CorrectRewardMax
                : rules.IncorrectRewardMax;

            return UnityEngine.Random.Range(minimum, maximum + 1);
        }

        private static bool CanMatchSymptoms(
            IReadOnlyList<SymptomDefinition> symptoms,
            IReadOnlyList<TreatmentId> selectedTreatments,
            int symptomIndex,
            bool[] usedTreatments)
        {
            if (symptomIndex >= symptoms.Count)
                return true;

            SymptomDefinition symptom = symptoms[symptomIndex];

            for (int treatmentIndex = 0;
                 treatmentIndex < selectedTreatments.Count;
                 treatmentIndex++)
            {
                if (usedTreatments[treatmentIndex]
                    || !symptom.Accepts(selectedTreatments[treatmentIndex]))
                {
                    continue;
                }

                usedTreatments[treatmentIndex] = true;

                if (CanMatchSymptoms(
                    symptoms,
                    selectedTreatments,
                    symptomIndex + 1,
                    usedTreatments))
                {
                    return true;
                }

                usedTreatments[treatmentIndex] = false;
            }

            return false;
        }
    }
}
