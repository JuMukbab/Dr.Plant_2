using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("UI")]
    public Transform content;

    [Header("Prefab")]
    public GameObject shopItemPrefab;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AddItem("물뿌리개", 100);
        AddItem("바이올린", 200);
        AddItem("가위", 300);
    }

    public void AddItem(string itemName, int price)
    {
        GameObject obj =
            Instantiate(shopItemPrefab, content);
            ShopItem item = obj.GetComponent<ShopItem>();

            item.Setup(itemName, price);
    }
}