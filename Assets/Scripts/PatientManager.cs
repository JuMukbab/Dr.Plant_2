using System;
using System.Collections.Generic;
using DrPlant.Data;
using DrPlant.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class PatientManager : MonoBehaviour
{
    public static PatientManager Instance;

    [HideInInspector] public GameObject[] patientPrefabs;

    public int wrongTreatmentCount;
    public int correctTreatmentCount;
    public int missedTreatmentCount;
    public int wrongCheckedTreatmentCount;

    public Transform spawnPoint;
    public Transform centerPoint;
    public Transform exitPoint;
    public Button nextButton;

    private DrPlantContentCatalog catalog;
    private GameObject currentPatient;
    private PatientCase currentCase;
    private bool patientLeaving;
    private bool patientReady;
    private string lastArrivalDialogue;
    private string lastGoodReview;
    private string lastBadReview;

    public bool PatientReady => patientReady && !patientLeaving && currentPatient != null;
    internal PatientCase CurrentCase => currentCase;
    internal GameObject CurrentPatient => currentPatient;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        catalog = DrPlantContent.Catalog;

        if (catalog == null)
        {
            enabled = false;

            if (nextButton != null)
                nextButton.interactable = false;

            return;
        }

        ChecklistManager.Instance?.EnsureInitialized();
        SpawnPatient();
    }

    public void SpawnPatient()
    {
        if (currentPatient != null || patientLeaving)
            return;

        ResetTreatmentCounters();
        patientReady = false;

        if (nextButton != null)
            nextButton.interactable = false;

        try
        {
            currentCase = PatientCaseGenerator.Create(catalog);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            enabled = false;
            return;
        }

        currentPatient = Instantiate(
            currentCase.Patient.Prefab,
            spawnPoint.position,
            Quaternion.identity);

        PlantStatus status = currentPatient.GetComponent<PlantStatus>();
        if (status == null)
        {
            Debug.LogError(
                $"Patient prefab {currentCase.Patient.DisplayName} is missing PlantStatus.");
            Destroy(currentPatient);
            currentPatient = null;
            enabled = false;
            return;
        }

        status.Initialize(currentCase);
        StatusBar.Instance?.SetTarget(status);
        ChecklistManager.Instance?.ResetSelections();

        PatientMove move = currentPatient.GetComponent<PatientMove>();
        if (move == null)
        {
            currentPatient.transform.position = centerPoint.position;
            HandlePatientArrived(currentPatient);
            return;
        }

        GameObject arrivingPatient = currentPatient;
        move.onArrive = () => HandlePatientArrived(arrivingPatient);
        move.MoveTo(centerPoint.position);
    }

    public void SendPatient()
    {
        if (!PatientReady || currentCase == null)
            return;

        patientLeaving = true;
        patientReady = false;

        if (nextButton != null)
            nextButton.interactable = false;

        List<TreatmentId> selectedTreatments =
            ChecklistManager.Instance != null
                ? ChecklistManager.Instance.GetCheckedTreatments()
                : new List<TreatmentId>();

        TreatmentOutcome outcome = TreatmentEvaluator.Evaluate(
            currentCase,
            selectedTreatments,
            catalog.Rules);

        UpdateTreatmentCounters(outcome.IsCorrect, selectedTreatments.Count);

        PlantStatus status = currentPatient.GetComponent<PlantStatus>();
        if (status != null)
            status.treated = true;

        if (MoneyManager.Instance != null)
            MoneyManager.Instance.AddMoney(outcome.Reward);

        ShowReview(outcome);
        TalkManager.Instance?.Clear();

        PatientMove move = currentPatient.GetComponent<PatientMove>();
        GameObject departingPatient = currentPatient;

        if (move == null)
        {
            CompleteDeparture(departingPatient);
            return;
        }

        move.onArrive = () => CompleteDeparture(departingPatient);
        move.MoveTo(exitPoint.position);
    }

    public void AddWrongTreatment()
    {
        wrongTreatmentCount++;
    }

    private void HandlePatientArrived(GameObject arrivingPatient)
    {
        if (arrivingPatient != currentPatient || patientLeaving)
            return;

        patientReady = true;

        if (nextButton != null)
            nextButton.interactable = true;

        string arrival = SelectNonRepeated(
            catalog.Dialogues.Arrival,
            ref lastArrivalDialogue);

        if (!string.IsNullOrEmpty(arrival))
            TalkManager.Instance?.Show(arrival);
    }

    private void ShowReview(TreatmentOutcome outcome)
    {
        IReadOnlyList<string> reviews = outcome.IsCorrect
            ? catalog.Dialogues.GoodReviews
            : catalog.Dialogues.BadReviews;

        string review = outcome.IsCorrect
            ? SelectNonRepeated(reviews, ref lastGoodReview)
            : SelectNonRepeated(reviews, ref lastBadReview);

        PatientReview patientReview = currentPatient.GetComponent<PatientReview>();
        if (patientReview != null)
            patientReview.ShowReview($"{review} (+{outcome.Reward} G)");
    }

    private void CompleteDeparture(GameObject departingPatient)
    {
        if (departingPatient != currentPatient)
            return;

        Destroy(departingPatient);
        currentPatient = null;
        currentCase = null;
        patientLeaving = false;

        SpawnPatient();
    }

    private void ResetTreatmentCounters()
    {
        wrongTreatmentCount = 0;
        correctTreatmentCount = 0;
        missedTreatmentCount = 0;
        wrongCheckedTreatmentCount = 0;
    }

    private void UpdateTreatmentCounters(bool isCorrect, int selectedCount)
    {
        if (isCorrect)
        {
            correctTreatmentCount = currentCase.Symptoms.Count;
            missedTreatmentCount = 0;
            wrongCheckedTreatmentCount = 0;
            return;
        }

        correctTreatmentCount = 0;
        missedTreatmentCount = currentCase.Symptoms.Count;
        wrongCheckedTreatmentCount = selectedCount;
    }

    private static string SelectNonRepeated(
        IReadOnlyList<string> values,
        ref string previous)
    {
        if (values == null || values.Count == 0)
            return string.Empty;

        int startIndex = UnityEngine.Random.Range(0, values.Count);

        for (int offset = 0; offset < values.Count; offset++)
        {
            string candidate = values[(startIndex + offset) % values.Count];

            if (values.Count == 1 || candidate != previous)
            {
                previous = candidate;
                return candidate;
            }
        }

        previous = values[startIndex];
        return previous;
    }
}
