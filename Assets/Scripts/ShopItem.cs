using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    private int itemPrice;
    public Image icon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    public void Setup(string name, int price)
    {
        itemName.text = name;
        priceText.text = price + " G";

        itemPrice = price;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(BuyItem);
    }
    void BuyItem()
    {
        if (MoneyManager.Instance.money < itemPrice)
        {
            Debug.Log("돈 부족");

            return;
        }

        MoneyManager.Instance.AddMoney(-itemPrice);

        Destroy(gameObject);
    }
}