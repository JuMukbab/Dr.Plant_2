using UnityEngine;

public class ChecklistUI : MonoBehaviour
{
    public static ChecklistUI Instance;

    void Awake()
    {
        Instance = this;

        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}