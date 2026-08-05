using UnityEngine;

public class ChecklistUI : MonoBehaviour
{
    public static ChecklistUI Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        ChecklistManager.Instance?.EnsureInitialized();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
