using System;
using System.Collections.Generic;
using DrPlant.Data;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class PatientVisualController : MonoBehaviour
{
    private const float PatientScaleMultiplier = 1.5f;
    private const float OvergrownScaleMultiplier = 2.2f;
    private const float SlowAnimationMultiplier = 2f;
    private const float BoredomRecoverySpeedMultiplier = 2.5f;

    [SerializeField] private Sprite[] animationFrames = Array.Empty<Sprite>();
    [SerializeField] private Sprite sunglassesSprite;

    private SpriteRenderer patientRenderer;
    private SpriteRenderer sunglassesRenderer;
    private Vector3 originalScale;
    private float baseCycleDuration = 0.75f;
    private float cycleDuration = 0.75f;
    private float elapsed;
    private int frameIndex;
    private bool hasBoredom;

    public bool IsConfigured => animationFrames != null
        && animationFrames.Length > 0;

    public void Configure(Sprite[] frames, Sprite sunglasses)
    {
        animationFrames = frames ?? Array.Empty<Sprite>();
        sunglassesSprite = sunglasses;
    }

    private void Awake()
    {
        patientRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        EnsureSunglassesRenderer();
    }

    public void Initialize(
        PatientDefinition patient,
        IReadOnlyList<SymptomDefinition> symptoms)
    {
        if (patientRenderer == null)
            patientRenderer = GetComponent<SpriteRenderer>();

        EnsureSunglassesRenderer();
        baseCycleDuration = Mathf.Max(0.1f, patient.AnimationSpeed);
        cycleDuration = baseCycleDuration;
        elapsed = 0f;
        frameIndex = 0;
        hasBoredom = HasSymptom(symptoms, SymptomId.Boredom);

        if (hasBoredom || HasSymptom(symptoms, SymptomId.Malnutrition))
            cycleDuration *= SlowAnimationMultiplier;

        patientRenderer.color = Color.white;

        if (HasSymptom(symptoms, SymptomId.Hot))
            patientRenderer.color = new Color(1f, 0.58f, 0.58f, 1f);
        else if (HasSymptom(symptoms, SymptomId.Cold))
            patientRenderer.color = new Color(0.58f, 0.76f, 1f, 1f);

        float symptomScale = HasSymptom(symptoms, SymptomId.Overgrown)
            ? OvergrownScaleMultiplier
            : 1f;
        transform.localScale = originalScale
            * PatientScaleMultiplier
            * symptomScale;

        if (animationFrames != null && animationFrames.Length > 0)
            patientRenderer.sprite = animationFrames[0];

        sunglassesRenderer.enabled = false;
    }

    public void ApplyTreatmentResult(
        bool isCorrect,
        IReadOnlyCollection<TreatmentId> selectedTreatments)
    {
        if (isCorrect && hasBoredom)
            cycleDuration = baseCycleDuration / BoredomRecoverySpeedMultiplier;

        sunglassesRenderer.enabled = Contains(
            selectedTreatments,
            TreatmentId.Sunglasses);
    }

    private void Update()
    {
        if (animationFrames == null || animationFrames.Length < 2)
            return;

        float frameDuration = cycleDuration / animationFrames.Length;
        elapsed += Time.deltaTime;

        while (elapsed >= frameDuration)
        {
            elapsed -= frameDuration;
            frameIndex = (frameIndex + 1) % animationFrames.Length;
            patientRenderer.sprite = animationFrames[frameIndex];
        }
    }

    private void EnsureSunglassesRenderer()
    {
        if (sunglassesRenderer != null)
            return;

        Transform accessory = transform.Find("SunglassesAccessory");
        if (accessory == null)
        {
            GameObject accessoryObject = new GameObject("SunglassesAccessory");
            accessory = accessoryObject.transform;
            accessory.SetParent(transform, false);
        }

        accessory.localPosition = new Vector3(0f, 0.66f, -0.01f);
        accessory.localRotation = Quaternion.identity;
        accessory.localScale = Vector3.one * 0.65f;

        sunglassesRenderer = accessory.GetComponent<SpriteRenderer>();
        if (sunglassesRenderer == null)
            sunglassesRenderer = accessory.gameObject.AddComponent<SpriteRenderer>();

        sunglassesRenderer.sprite = sunglassesSprite;
        sunglassesRenderer.enabled = false;

        if (patientRenderer != null)
        {
            sunglassesRenderer.sortingLayerID = patientRenderer.sortingLayerID;
            sunglassesRenderer.sortingOrder = patientRenderer.sortingOrder + 1;
        }
    }

    private static bool HasSymptom(
        IReadOnlyList<SymptomDefinition> symptoms,
        SymptomId symptomId)
    {
        if (symptoms == null)
            return false;

        for (int index = 0; index < symptoms.Count; index++)
        {
            if (symptoms[index].Id == symptomId)
                return true;
        }

        return false;
    }

    private static bool Contains(
        IReadOnlyCollection<TreatmentId> treatments,
        TreatmentId treatmentId)
    {
        if (treatments == null)
            return false;

        foreach (TreatmentId treatment in treatments)
        {
            if (treatment == treatmentId)
                return true;
        }

        return false;
    }
}
