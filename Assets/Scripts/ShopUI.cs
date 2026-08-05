using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public GameObject shopPanel;

    void Start()
    {
        shopPanel.SetActive(false);
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }
}