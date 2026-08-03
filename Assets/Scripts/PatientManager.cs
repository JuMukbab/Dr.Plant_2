using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class PatientManager : MonoBehaviour
{
    public GameObject[] patientPrefabs;

    GameObject currentPatient;
    public static PatientManager Instance;
    public int wrongTreatmentCount = 0;

    public int correctTreatmentCount;
    public int missedTreatmentCount;
    public int wrongCheckedTreatmentCount;
    public Transform spawnPoint;
    public Transform centerPoint;
    public Transform exitPoint;
    bool patientLeaving = false;
    public Button nextButton;
    void Start()
    {
        SpawnPatient();
    }
    void Awake()
    {
        Instance = this;
    }
    void EvaluateTreatment()
    {
        PlantStatus status =
            currentPatient.GetComponent<PlantStatus>();
        List<string> playerTreatments =
            ChecklistManager.Instance.GetCheckedTreatments();
        
        correctTreatmentCount = 0;
        wrongCheckedTreatmentCount = 0;
        missedTreatmentCount = 0;

        foreach(string treatment in status.requiredTreatments)
        {
            if(playerTreatments.Contains(treatment))
            {
                correctTreatmentCount++;
            }
            else
            {
                missedTreatmentCount++;
            }
        }
        foreach (string treatment in playerTreatments)
        {
            if (!status.requiredTreatments.Contains(treatment))
            {
                wrongCheckedTreatmentCount++;
            }
        }
        // Debug.Log("==========");

        // Debug.Log("맞은 치료 : " + correctTreatmentCount);
        // Debug.Log("빠뜨린 치료 : " + missedTreatmentCount);
        // Debug.Log("잘못한 치료 : " + wrongCheckedTreatmentCount);

        // Debug.Log("==========");
    }
    int CalculateScore(PlantStatus status)
    {
        int score = 100;

        // HP
        if (status.hp < 60)
            score -= 30;

        // 습도
        if (status.humidity < 30)
            score -= 20;

        if (status.humidity > 90)
            score -= 10;

        // 지루함
        if (status.boredom > 80)
            score -= 20;

        // 죽었으면 0점
        if (status.isDead)
            score = 0;

        score -= wrongCheckedTreatmentCount * 15;

        score -= missedTreatmentCount * 20;

        score += correctTreatmentCount * 5;

        return Mathf.Clamp(score, 0, 100);
    }
    public void AddWrongTreatment()
    {
        wrongTreatmentCount++;

        Debug.Log("잘못된 치료 : " + wrongTreatmentCount);
    }
        

    public void SpawnPatient()
    {
        wrongTreatmentCount = 0;
        int random = Random.Range(0, patientPrefabs.Length);

        currentPatient =
            Instantiate(patientPrefabs[random],
                        spawnPoint.position,
                        Quaternion.identity);
        StatusBar.Instance.SetTarget(
            currentPatient.GetComponent<PlantStatus>());

        PatientMove move =
            currentPatient.GetComponent<PatientMove>();

        move.MoveTo(centerPoint.position);
    }

    public void SendPatient()
    {
        if (currentPatient == null)
            return;

        if (patientLeaving)
            return;

        patientLeaving = true;

        nextButton.interactable = false;

        PlantStatus status =
            currentPatient.GetComponent<PlantStatus>();

        EvaluateTreatment();

        int score = CalculateScore(status);

        PatientReview review =
            currentPatient.GetComponent<PatientReview>();

        review.ShowReview(GetReview());

        MoneyManager.Instance.AddMoney(score);

        PatientMove move =
            currentPatient.GetComponent<PatientMove>();

        move.MoveTo(exitPoint.position);

        move.onArrive = () =>
        {
            Destroy(currentPatient);

            currentPatient = null;

            SpawnPatient();

            patientLeaving = false;

            nextButton.interactable = true;
        };
    }

    string GetReview()
    {
        if(wrongCheckedTreatmentCount == 0 &&
        missedTreatmentCount == 0)
        {
            string[] reviews =
            {
                "정말 감사합니다!",
                "몸이 훨씬 좋아졌어요!",
                "최고의 치료였어요!"
            };

            return reviews[Random.Range(0,reviews.Length)];
        }

        string[] badReviews =
        {
            "더는 못 마시겠어요...",
            "조금 아팠어요...",
            "다음엔 더 잘 부탁드려요..."
        };

        return badReviews[Random.Range(0,badReviews.Length)];
    }
    
}