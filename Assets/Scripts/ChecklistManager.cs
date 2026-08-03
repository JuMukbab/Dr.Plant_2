using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChecklistManager : MonoBehaviour
{
    public static ChecklistManager Instance;
    public List<Toggle> toggles = new List<Toggle>();
    public Transform content;

    public GameObject togglePrefab;

    void Start()
    {
        Debug.Log("checklistmanager START");
        AddTreatment("물 주기");
        AddTreatment("음악 들려주기");
        AddTreatment("잎 닦기");
        AddTreatment("햇빛 쬐어주기");
    }
    void Awake()
    {
        Instance = this;
    }
    public void AddTreatment(string treatmentName)
    {
        GameObject obj =
            Instantiate(togglePrefab, content);

        TextMeshProUGUI text =
            obj.GetComponentInChildren<TextMeshProUGUI>();

        text.text = treatmentName;
        Toggle toggle =
            obj.GetComponent<Toggle>();

        toggle.isOn = false;

        toggles.Add(toggle);
    }
    public List<string> GetCheckedTreatments()
    {
        List<string> result = new List<string>();

        foreach(Toggle toggle in toggles)
        {
            if(toggle.isOn)
            {
                TextMeshProUGUI text =
                    toggle.GetComponentInChildren<TextMeshProUGUI>();

                result.Add(text.text);
            }
        }

        return result;
    }
}