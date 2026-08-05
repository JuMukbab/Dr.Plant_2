using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public GameObject shopPanel;

    private void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void OpenShop()
    {
        ShopManager.Instance?.RefreshItems();

        if (shopPanel != null)
            shopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
}
