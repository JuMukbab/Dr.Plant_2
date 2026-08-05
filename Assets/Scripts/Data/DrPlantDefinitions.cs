using System;
using System.Collections.Generic;
using UnityEngine;

namespace DrPlant.Data
{
    public enum PatientVoiceWaveform
    {
        Sine,
        Square,
        Triangle
    }

    [Serializable]
    public sealed class PatientVoiceProfile
    {
        [SerializeField, Min(80f)] private float frequency = 500f;
        [SerializeField] private PatientVoiceWaveform waveform = PatientVoiceWaveform.Sine;
        [SerializeField, Range(0f, 1f)] private float volume = 0.04f;
        [SerializeField, Min(0.01f)] private float duration = 0.045f;
        [SerializeField, Min(0f)] private float pitchVariation = 40f;

        public float Frequency => frequency;
        public PatientVoiceWaveform Waveform => waveform;
        public float Volume => volume;
        public float Duration => duration;
        public float PitchVariation => pitchVariation;

        public PatientVoiceProfile(
            float frequency,
            PatientVoiceWaveform waveform,
            float volume,
            float duration,
            float pitchVariation)
        {
            this.frequency = frequency;
            this.waveform = waveform;
            this.volume = volume;
            this.duration = duration;
            this.pitchVariation = pitchVariation;
        }
    }

    [Serializable]
    public sealed class PatientDefinition
    {
        [SerializeField] private PatientId id;
        [SerializeField] private string displayName;
        [SerializeField] private GameObject prefab;
        [SerializeField, Min(0.1f)] private float animationSpeed = 0.75f;
        [SerializeField, Min(0.1f)] private float displayScale = 6.5f;
        [SerializeField] private PatientVoiceProfile voice;

        public PatientId Id => id;
        public string DisplayName => displayName;
        public GameObject Prefab => prefab;
        public float AnimationSpeed => animationSpeed;
        public float DisplayScale => displayScale;
        public PatientVoiceProfile Voice => voice;
        public bool IsPlayable => prefab != null;

        public PatientDefinition(
            PatientId id,
            string displayName,
            GameObject prefab,
            float animationSpeed,
            float displayScale,
            PatientVoiceProfile voice)
        {
            this.id = id;
            this.displayName = displayName;
            this.prefab = prefab;
            this.animationSpeed = animationSpeed;
            this.displayScale = displayScale;
            this.voice = voice;
        }
    }

    [Serializable]
    public sealed class SymptomDefinition
    {
        [SerializeField] private SymptomId id;
        [SerializeField] private string displayName;
        [SerializeField] private TreatmentId[] acceptedTreatments = Array.Empty<TreatmentId>();
        [SerializeField] private ShopItemId requiredShopItem;
        [SerializeField] private SymptomId[] incompatibleSymptoms = Array.Empty<SymptomId>();
        [SerializeField] private string[] dialogues = Array.Empty<string>();

        public SymptomId Id => id;
        public string DisplayName => displayName;
        public IReadOnlyList<TreatmentId> AcceptedTreatments => acceptedTreatments;
        public ShopItemId RequiredShopItem => requiredShopItem;
        public IReadOnlyList<SymptomId> IncompatibleSymptoms => incompatibleSymptoms;
        public IReadOnlyList<string> Dialogues => dialogues;

        public SymptomDefinition(
            SymptomId id,
            string displayName,
            TreatmentId[] acceptedTreatments,
            ShopItemId requiredShopItem,
            SymptomId[] incompatibleSymptoms,
            string[] dialogues)
        {
            this.id = id;
            this.displayName = displayName;
            this.acceptedTreatments = acceptedTreatments ?? Array.Empty<TreatmentId>();
            this.requiredShopItem = requiredShopItem;
            this.incompatibleSymptoms = incompatibleSymptoms ?? Array.Empty<SymptomId>();
            this.dialogues = dialogues ?? Array.Empty<string>();
        }

        public bool Accepts(TreatmentId treatmentId)
        {
            return Array.IndexOf(acceptedTreatments, treatmentId) >= 0;
        }
    }

    [Serializable]
    public sealed class TreatmentDefinition
    {
        [SerializeField] private TreatmentId id;
        [SerializeField] private string displayName;
        [SerializeField] private ShopItemId requiredShopItem;

        public TreatmentId Id => id;
        public string DisplayName => displayName;
        public ShopItemId RequiredShopItem => requiredShopItem;

        public TreatmentDefinition(
            TreatmentId id,
            string displayName,
            ShopItemId requiredShopItem)
        {
            this.id = id;
            this.displayName = displayName;
            this.requiredShopItem = requiredShopItem;
        }
    }

    [Serializable]
    public sealed class ShopItemDefinition
    {
        [SerializeField] private ShopItemId id;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField, Min(0)] private int price;
        [SerializeField] private TreatmentId unlockedTreatment;

        public ShopItemId Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public int Price => price;
        public TreatmentId UnlockedTreatment => unlockedTreatment;

        public ShopItemDefinition(
            ShopItemId id,
            string displayName,
            string description,
            int price,
            TreatmentId unlockedTreatment)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
            this.price = price;
            this.unlockedTreatment = unlockedTreatment;
        }
    }

    [Serializable]
    public sealed class ClinicRules
    {
        [SerializeField, Range(0f, 1f)] private float compositeSymptomChance = 0.3f;
        [SerializeField, Range(0f, 1f)] private float normalDialogueChance = 0.3f;
        [SerializeField, Min(1)] private int maxSymptoms = 2;
        [SerializeField, Min(0)] private int correctRewardMin = 150;
        [SerializeField, Min(0)] private int correctRewardMax = 200;
        [SerializeField, Min(0)] private int incorrectRewardMin = 5;
        [SerializeField, Min(0)] private int incorrectRewardMax = 10;

        public float CompositeSymptomChance => compositeSymptomChance;
        public float NormalDialogueChance => normalDialogueChance;
        public int MaxSymptoms => maxSymptoms;
        public int CorrectRewardMin => correctRewardMin;
        public int CorrectRewardMax => correctRewardMax;
        public int IncorrectRewardMin => incorrectRewardMin;
        public int IncorrectRewardMax => incorrectRewardMax;

        public ClinicRules(
            float compositeSymptomChance,
            float normalDialogueChance,
            int maxSymptoms,
            int correctRewardMin,
            int correctRewardMax,
            int incorrectRewardMin,
            int incorrectRewardMax)
        {
            this.compositeSymptomChance = compositeSymptomChance;
            this.normalDialogueChance = normalDialogueChance;
            this.maxSymptoms = maxSymptoms;
            this.correctRewardMin = correctRewardMin;
            this.correctRewardMax = correctRewardMax;
            this.incorrectRewardMin = incorrectRewardMin;
            this.incorrectRewardMax = incorrectRewardMax;
        }
    }

    [Serializable]
    public sealed class DialogueLibrary
    {
        [SerializeField] private string[] arrival = Array.Empty<string>();
        [SerializeField] private string[] normal = Array.Empty<string>();
        [SerializeField] private string[] goodReviews = Array.Empty<string>();
        [SerializeField] private string[] badReviews = Array.Empty<string>();

        public IReadOnlyList<string> Arrival => arrival;
        public IReadOnlyList<string> Normal => normal;
        public IReadOnlyList<string> GoodReviews => goodReviews;
        public IReadOnlyList<string> BadReviews => badReviews;

        public DialogueLibrary(
            string[] arrival,
            string[] normal,
            string[] goodReviews,
            string[] badReviews)
        {
            this.arrival = arrival ?? Array.Empty<string>();
            this.normal = normal ?? Array.Empty<string>();
            this.goodReviews = goodReviews ?? Array.Empty<string>();
            this.badReviews = badReviews ?? Array.Empty<string>();
        }
    }
}
