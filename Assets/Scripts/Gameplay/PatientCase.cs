using System;
using System.Collections.Generic;
using DrPlant.Data;

namespace DrPlant.Gameplay
{
    public sealed class PatientCase
    {
        private readonly SymptomDefinition[] symptoms;

        public PatientDefinition Patient { get; }
        public IReadOnlyList<SymptomDefinition> Symptoms => symptoms;

        public PatientCase(
            PatientDefinition patient,
            IEnumerable<SymptomDefinition> symptoms)
        {
            Patient = patient ?? throw new ArgumentNullException(nameof(patient));
            this.symptoms = symptoms == null
                ? Array.Empty<SymptomDefinition>()
                : new List<SymptomDefinition>(symptoms).ToArray();

            if (this.symptoms.Length == 0)
                throw new ArgumentException("A patient case needs at least one symptom.", nameof(symptoms));

            for (int index = 0; index < this.symptoms.Length; index++)
            {
                if (this.symptoms[index] == null)
                    throw new ArgumentException("Symptoms cannot contain null entries.", nameof(symptoms));
            }
        }

        public bool HasSymptom(SymptomId symptomId)
        {
            for (int index = 0; index < symptoms.Length; index++)
            {
                if (symptoms[index].Id == symptomId)
                    return true;
            }

            return false;
        }
    }
}
