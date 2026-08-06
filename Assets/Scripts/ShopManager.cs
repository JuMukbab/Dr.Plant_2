using System.Collections.Generic;
using DrPlant.Data;
using DrPlant.Progression;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("UI")]
    public Transform content;

    [Header("Prefab")]
    public GameObject shopItemPrefab;

    private readonly Dictionary<ShopItemId, ShopItem> items =
        new Dictionary<ShopItemId, ShopItem>();

    internal int ItemCount => items.Count;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ClinicProgress.Instance.Changed += RefreshItems;
        ClinicProgress.Instance.InventoryChanged += HandleInventoryChanged;
        RebuildItems();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        ClinicProgress.Instance.Changed -= RefreshItems;
        ClinicProgress.Instance.InventoryChanged -= HandleInventoryChanged;
    }

    public void RebuildItems()
    {
        ClearItems();

        DrPlantContentCatalog catalog = DrPlantContent.Catalog;
        if (catalog == null || content == null || shopItemPrefab == null)
        {
            Debug.LogError("ShopManager is missing its catalog or UI references.");
            return;
        }

        foreach (ShopItemDefinition definition in catalog.ShopItems)
        {
            GameObject itemObject = Instantiate(shopItemPrefab, content);
            ShopItem item = itemObject.GetComponent<ShopItem>();

            if (item == null)
            {
                Debug.LogError("Shop item prefab needs a ShopItem component.");
                Destroy(itemObject);
                continue;
            }

            item.Setup(definition);
            items.Add(definition.Id, item);
        }

        RefreshItems();
    }

    public PurchaseResult TryPurchase(ShopItemId itemId)
    {
        DrPlantContentCatalog catalog = DrPlantContent.Catalog;
        if (catalog == null
            || !catalog.TryGetShopItem(itemId, out ShopItemDefinition definition))
        {
            return PurchaseResult.UnknownItem;
        }

        PurchaseResult result = ClinicProgress.Instance.TryPurchase(definition);

        if (result == PurchaseResult.Success)
            ClinicAudioManager.Instance?.PlayPurchase();

        return result;
    }

    public void RefreshItems()
    {
        ClinicProgress progress = ClinicProgress.Instance;

        foreach (ShopItem item in items.Values)
        {
            if (item != null)
                item.Refresh(progress);
        }
    }

    private void HandleInventoryChanged()
    {
        ChecklistManager.Instance?.RefreshFromProgress();
    }

    private void ClearItems()
    {
        foreach (ShopItem item in items.Values)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        items.Clear();
    }
}
