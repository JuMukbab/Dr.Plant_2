using DrPlant.Progression;
using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    public TMP_Text moneyText;
    public int money;

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

    public void AddMoney(int amount)
    {
        ClinicProgress.Instance.AddMoney(amount);
    }

    private void RefreshFromProgress()
    {
        money = ClinicProgress.Instance.Money;

        if (moneyText != null)
            moneyText.text = $"{money} G";
    }
}
