using System.Collections;
using System.Text;
using DrPlant.Gameplay;
using DrPlant.Progression;
using TMPro;
using UnityEngine;

public sealed class ClinicHud : MonoBehaviour
{
    [SerializeField] private TMP_Text patientText;
    [SerializeField] private TMP_Text symptomText;
    [SerializeField] private TMP_Text progressText;

    private PatientManager patientManager;
    private Coroutine bindRoutine;

    public bool IsConfigured =>
        patientText != null && symptomText != null && progressText != null;

    public void Configure(
        TMP_Text patientLabel,
        TMP_Text symptomLabel,
        TMP_Text progressLabel)
    {
        patientText = patientLabel;
        symptomText = symptomLabel;
        progressText = progressLabel;
    }

    private void OnEnable()
    {
        ClinicProgress.Instance.Changed += RefreshProgress;
        RefreshProgress();

        if (bindRoutine == null)
            bindRoutine = StartCoroutine(BindPatientManager());
    }

    private void OnDisable()
    {
        ClinicProgress.Instance.Changed -= RefreshProgress;

        if (patientManager != null)
            patientManager.ActiveCaseChanged -= RefreshCase;

        patientManager = null;

        if (bindRoutine != null)
        {
            StopCoroutine(bindRoutine);
            bindRoutine = null;
        }
    }

    private IEnumerator BindPatientManager()
    {
        while (PatientManager.Instance == null)
            yield return null;

        patientManager = PatientManager.Instance;
        patientManager.ActiveCaseChanged += RefreshCase;
        RefreshCase(patientManager.ActiveCase);
        bindRoutine = null;
    }

    private void RefreshCase(PatientCase patientCase)
    {
        if (patientText == null || symptomText == null)
            return;

        if (patientCase == null)
        {
            patientText.text = "환자  대기 중";
            symptomText.text = "증상  확인 중";
            return;
        }

        patientText.text = $"환자  {patientCase.Patient.DisplayName}";

        StringBuilder symptoms = new StringBuilder();
        for (int index = 0; index < patientCase.Symptoms.Count; index++)
        {
            if (index > 0)
                symptoms.Append(", ");

            symptoms.Append(patientCase.Symptoms[index].DisplayName);
        }

        symptomText.text = $"증상  {symptoms}";
    }

    private void RefreshProgress()
    {
        if (progressText == null)
            return;

        ClinicProgress progress = ClinicProgress.Instance;
        progressText.text =
            $"진료 {progress.TreatedPatientCount}명  |  도구 {progress.PurchasedItemCount}/3";
    }
}
