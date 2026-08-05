using System;
using System.Collections.Generic;
using DrPlant.Data;
using UnityEngine;

namespace DrPlant.Progression
{
    [Serializable]
    public sealed class ClinicSaveData
    {
        public int version = 1;
        public int money;
        public int treatedPatientCount;
        public List<ShopItemId> purchasedShopItems = new List<ShopItemId>();
    }

    public enum PurchaseResult
    {
        Success,
        AlreadyPurchased,
        InsufficientFunds,
        UnknownItem
    }

    public sealed class ClinicProgress
    {
        public const string DefaultSaveKey = "DrPlant.ClinicProgress.v1";

        private const string SmokeSaveKey = "DrPlant.ClinicProgress.SmokeTest";
        private const string SmokeCommandLineFlag = "-drplant-smoke-test";

        private static ClinicProgress instance;

        private readonly string saveKey;
        private readonly HashSet<ShopItemId> purchasedShopItems =
            new HashSet<ShopItemId>();

        private ClinicSaveData data;

        public static ClinicProgress Instance =>
            instance ?? (instance = new ClinicProgress(ResolveDefaultSaveKey()));

        public int Money => data.money;
        public int TreatedPatientCount => data.treatedPatientCount;
        public int PurchasedItemCount => purchasedShopItems.Count;

        public event Action Changed;
        public event Action InventoryChanged;

        public ClinicProgress(string saveKey)
        {
            if (string.IsNullOrWhiteSpace(saveKey))
                throw new ArgumentException("A save key is required.", nameof(saveKey));

            this.saveKey = saveKey;
            LoadData();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSharedInstance()
        {
            instance = null;
        }

        public bool IsPurchased(ShopItemId itemId)
        {
            return purchasedShopItems.Contains(itemId);
        }

        public HashSet<ShopItemId> GetPurchasedShopItems()
        {
            return new HashSet<ShopItemId>(purchasedShopItems);
        }

        public void AddMoney(int amount)
        {
            int updatedMoney = AddClamped(data.money, amount);
            if (updatedMoney == data.money)
                return;

            data.money = updatedMoney;
            Commit(false);
        }

        public void CompleteTreatment(int reward)
        {
            data.money = AddClamped(data.money, reward);
            data.treatedPatientCount = AddClamped(data.treatedPatientCount, 1);
            Commit(false);
        }

        public void RecordTreatment()
        {
            data.treatedPatientCount = AddClamped(data.treatedPatientCount, 1);
            Commit(false);
        }

        public PurchaseResult TryPurchase(ShopItemDefinition definition)
        {
            if (definition == null || definition.Id == ShopItemId.None)
                return PurchaseResult.UnknownItem;

            if (purchasedShopItems.Contains(definition.Id))
                return PurchaseResult.AlreadyPurchased;

            if (data.money < definition.Price)
                return PurchaseResult.InsufficientFunds;

            data.money -= definition.Price;
            purchasedShopItems.Add(definition.Id);
            Commit(true);
            return PurchaseResult.Success;
        }

        public void Reload()
        {
            LoadData();
            Changed?.Invoke();
            InventoryChanged?.Invoke();
        }

        public void Reset()
        {
            data = new ClinicSaveData();
            purchasedShopItems.Clear();
            Commit(true);
        }

        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(saveKey);
            PlayerPrefs.Save();

            data = new ClinicSaveData();
            purchasedShopItems.Clear();
            Changed?.Invoke();
            InventoryChanged?.Invoke();
        }

        private void LoadData()
        {
            ClinicSaveData loadedData = null;
            string json = PlayerPrefs.GetString(saveKey, string.Empty);

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    loadedData = JsonUtility.FromJson<ClinicSaveData>(json);
                }
                catch (ArgumentException exception)
                {
                    Debug.LogWarning($"Ignoring invalid Dr.Plant save data: {exception.Message}");
                }
            }

            data = loadedData ?? new ClinicSaveData();
            data.version = 1;
            data.money = Mathf.Max(0, data.money);
            data.treatedPatientCount = Mathf.Max(0, data.treatedPatientCount);

            purchasedShopItems.Clear();

            if (data.purchasedShopItems == null)
                data.purchasedShopItems = new List<ShopItemId>();

            foreach (ShopItemId itemId in data.purchasedShopItems)
            {
                if (itemId != ShopItemId.None)
                    purchasedShopItems.Add(itemId);
            }
        }

        private void Commit(bool inventoryChanged)
        {
            data.purchasedShopItems = new List<ShopItemId>(purchasedShopItems);
            data.purchasedShopItems.Sort();
            PlayerPrefs.SetString(saveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();

            Changed?.Invoke();

            if (inventoryChanged)
                InventoryChanged?.Invoke();
        }

        private static int AddClamped(int current, int amount)
        {
            long result = (long)current + amount;
            return (int)Math.Max(0L, Math.Min(int.MaxValue, result));
        }

        private static string ResolveDefaultSaveKey()
        {
            string[] arguments = Environment.GetCommandLineArgs();

            for (int index = 0; index < arguments.Length; index++)
            {
                if (string.Equals(
                    arguments[index],
                    SmokeCommandLineFlag,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return SmokeSaveKey;
                }
            }

            return DefaultSaveKey;
        }
    }
}
