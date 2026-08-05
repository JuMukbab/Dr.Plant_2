using DrPlant.Progression;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int money;
    public int treatedPlant;
    public bool gameOver;

    private void Awake()
    {
        Instance = this;
        ClinicProgress.Instance.Changed += RefreshFromProgress;
        RefreshFromProgress();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        ClinicProgress.Instance.Changed -= RefreshFromProgress;
    }

    public void AddMoney(int value)
    {
        ClinicProgress.Instance.AddMoney(value);
    }

    public void PlantSaved()
    {
        ClinicProgress.Instance.RecordTreatment();
    }

    public void GameOver()
    {
        gameOver = true;
    }

    private void RefreshFromProgress()
    {
        money = ClinicProgress.Instance.Money;
        treatedPlant = ClinicProgress.Instance.TreatedPatientCount;
    }
}
