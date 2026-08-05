using DrPlant.Data;
using DrPlant.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private ShopItemDefinition definition;

    internal ShopItemId ItemId =>
        definition != null ? definition.Id : ShopItemId.None;

    public void Setup(ShopItemDefinition itemDefinition)
    {
        definition = itemDefinition;

        if (definition == null)
        {
            Debug.LogError("ShopItem cannot be set up without a definition.");
            return;
        }

        if (itemName != null)
            itemName.text = definition.DisplayName;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyItem);
        }

        Refresh(ClinicProgress.Instance);
    }

    public void Refresh(ClinicProgress progress)
    {
        if (definition == null || progress == null)
            return;

        bool purchased = progress.IsPurchased(definition.Id);
        bool affordable = progress.Money >= definition.Price;

        if (priceText != null)
        {
            priceText.text = purchased
                ? "구매 완료"
                : $"{definition.Price} G";
        }

        if (buyButton != null)
            buyButton.interactable = !purchased && affordable;
    }

    private void BuyItem()
    {
        if (definition == null || ShopManager.Instance == null)
            return;

        PurchaseResult result = ShopManager.Instance.TryPurchase(definition.Id);

        if (result == PurchaseResult.InsufficientFunds)
            Debug.Log($"골드가 부족합니다: {definition.DisplayName}");
    }
}
