using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    public TMP_Text moneyText;

    public int money = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateMoneyUI();
    }

    void UpdateMoneyUI()
    {
        moneyText.text = "$ " + money;
    }

    public void AddMoney(int amount)
    {
        money += amount;

        moneyText.text = money + " G";
    }
}