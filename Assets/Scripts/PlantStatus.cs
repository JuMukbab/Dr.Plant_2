using System.Collections;
using System.Collections.Generic;
using DrPlant.Data;
using DrPlant.Gameplay;
using UnityEngine;

public class PlantStatus : MonoBehaviour
{
    [HideInInspector] public List<string> requiredTreatments = new List<string>();

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite deadSprite;

    [Header("Status")]
    public float hp = 100f;
    public float humidity = 50f;
    public float temperature = 50f;
    public float boredom = 30f;
    public bool diagnosed;
    public bool treated;
    public bool isDead;
    public string currentMessage = "...";

    private SpriteRenderer spriteRenderer;
    private PatientCase patientCase;
    private string lastDialogue;

    public PatientCase PatientCase => patientCase;

    private void Awake()
    {
        EnsureSpriteRenderer();
    }

    private void Start()
    {
        EnsureSpriteRenderer();

        if (patientCase == null)
            SetHealthyDefaults();
    }

    public void Initialize(PatientCase newCase)
    {
        patientCase = newCase;
        diagnosed = false;
        treated = false;
        isDead = false;
        currentMessage = "...";
        lastDialogue = string.Empty;

        EnsureSpriteRenderer();
        SetHealthyDefaults();
        ApplySymptoms(newCase.Symptoms);
    }

    public string GetDialogue()
    {
        DrPlantContentCatalog catalog = DrPlantContent.Catalog;

        if (catalog == null || patientCase == null)
            return currentMessage;

        IReadOnlyList<string> dialoguePool = catalog.Dialogues.Normal;
        bool useSymptomDialogue =
            UnityEngine.Random.value >= catalog.Rules.NormalDialogueChance;

        if (useSymptomDialogue && patientCase.Symptoms.Count > 0)
        {
            SymptomDefinition symptom =
                patientCase.Symptoms[
                    UnityEngine.Random.Range(0, patientCase.Symptoms.Count)];

            if (symptom.Dialogues.Count > 0)
                dialoguePool = symptom.Dialogues;
        }

        currentMessage = SelectNonRepeated(dialoguePool, lastDialogue);
        lastDialogue = currentMessage;
        return currentMessage;
    }

    public IEnumerator ShockEffect()
    {
        EnsureSpriteRenderer();

        for (int index = 0; index < 6; index++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.08f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.08f);
        }
    }

    public void Revive()
    {
        EnsureSpriteRenderer();

        isDead = false;
        hp = 40f;
        spriteRenderer.sprite = normalSprite;
        currentMessage = "...";
    }

    private void SetHealthyDefaults()
    {
        hp = UnityEngine.Random.Range(70f, 96f);
        humidity = UnityEngine.Random.Range(45f, 76f);
        temperature = UnityEngine.Random.Range(40f, 61f);
        boredom = UnityEngine.Random.Range(10f, 41f);
    }

    private void ApplySymptoms(IReadOnlyList<SymptomDefinition> symptoms)
    {
        for (int index = 0; index < symptoms.Count; index++)
        {
            switch (symptoms[index].Id)
            {
                case SymptomId.Dehydration:
                    humidity = UnityEngine.Random.Range(5f, 21f);
                    break;

                case SymptomId.Hot:
                    temperature = UnityEngine.Random.Range(82f, 101f);
                    break;

                case SymptomId.Cold:
                    temperature = UnityEngine.Random.Range(0f, 16f);
                    break;

                case SymptomId.Malnutrition:
                    hp = UnityEngine.Random.Range(30f, 51f);
                    break;

                case SymptomId.Boredom:
                    boredom = UnityEngine.Random.Range(82f, 101f);
                    break;

                case SymptomId.Overgrown:
                    hp = Mathf.Min(hp, UnityEngine.Random.Range(55f, 71f));
                    break;
            }
        }
    }

    private void EnsureSpriteRenderer()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && normalSprite == null)
            normalSprite = spriteRenderer.sprite;
    }

    private static string SelectNonRepeated(
        IReadOnlyList<string> values,
        string previous)
    {
        if (values == null || values.Count == 0)
            return "...";

        int startIndex = UnityEngine.Random.Range(0, values.Count);

        for (int offset = 0; offset < values.Count; offset++)
        {
            string candidate = values[(startIndex + offset) % values.Count];

            if (values.Count == 1 || candidate != previous)
                return candidate;
        }

        return values[startIndex];
    }
}
