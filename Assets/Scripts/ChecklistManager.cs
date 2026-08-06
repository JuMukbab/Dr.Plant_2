using System.Collections.Generic;
using DrPlant.Data;
using DrPlant.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChecklistManager : MonoBehaviour
{
    public static ChecklistManager Instance;

    public List<Toggle> toggles = new List<Toggle>();
    public Transform content;
    public GameObject togglePrefab;

    private readonly Dictionary<Toggle, TreatmentId> treatmentIds =
        new Dictionary<Toggle, TreatmentId>();
    private bool initialized;

    internal int ItemCount => treatmentIds.Count;

    private void Awake()
    {
        Instance = this;
    }

    public void EnsureInitialized()
    {
        if (!initialized)
            RefreshFromProgress();
    }

    public void RefreshFromProgress()
    {
        RebuildAvailableTreatments(
            ClinicProgress.Instance.GetPurchasedShopItems());
    }

    public void RebuildAvailableTreatments(
        ISet<ShopItemId> purchasedShopItems)
    {
        ClearItems();

        DrPlantContentCatalog catalog = DrPlantContent.Catalog;
        if (catalog == null || content == null || togglePrefab == null)
        {
            Debug.LogError("ChecklistManager is missing its catalog or UI references.");
            return;
        }

        foreach (TreatmentDefinition treatment in catalog.Treatments)
        {
            if (catalog.IsTreatmentUnlocked(treatment.Id, purchasedShopItems))
                AddTreatment(treatment);
        }

        initialized = true;
    }

    public List<TreatmentId> GetCheckedTreatments()
    {
        EnsureInitialized();

        List<TreatmentId> result = new List<TreatmentId>();

        foreach (Toggle toggle in toggles)
        {
            if (toggle != null
                && toggle.isOn
                && treatmentIds.TryGetValue(toggle, out TreatmentId treatmentId))
            {
                result.Add(treatmentId);
            }
        }

        return result;
    }

    public void ResetSelections()
    {
        EnsureInitialized();

        foreach (Toggle toggle in toggles)
        {
            if (toggle != null)
                toggle.SetIsOnWithoutNotify(false);
        }
    }

    internal void SetSelectedTreatments(ISet<TreatmentId> selectedTreatments)
    {
        EnsureInitialized();

        foreach (KeyValuePair<Toggle, TreatmentId> entry in treatmentIds)
        {
            bool isSelected = selectedTreatments != null
                && selectedTreatments.Contains(entry.Value);

            entry.Key.SetIsOnWithoutNotify(isSelected);
        }
    }

    private void AddTreatment(TreatmentDefinition treatment)
    {
        GameObject itemObject = Instantiate(togglePrefab, content);
        Toggle toggle = itemObject.GetComponent<Toggle>();
        TextMeshProUGUI label = itemObject.GetComponentInChildren<TextMeshProUGUI>();

        if (toggle == null || label == null)
        {
            Debug.LogError("Treatment toggle prefab needs a Toggle and TextMeshProUGUI.");
            Destroy(itemObject);
            return;
        }

        label.text = treatment.DisplayName;
        toggle.SetIsOnWithoutNotify(false);

        toggle.onValueChanged.AddListener(HandleToggleChanged);

        toggles.Add(toggle);
        treatmentIds.Add(toggle, treatment.Id);
    }

    private void ClearItems()
    {
        foreach (Toggle toggle in toggles)
        {
            if (toggle != null)
                Destroy(toggle.gameObject);
        }

        toggles.Clear();
        treatmentIds.Clear();
        initialized = false;
    }

    private static void HandleToggleChanged(bool selected)
    {
        ClinicAudioManager.Instance?.PlayChecklistToggle(selected);
    }
}
