using UnityEngine;

public class PlantInteraction : MonoBehaviour
{
    private PlantStatus status;

    private void Awake()
    {
        status = GetComponent<PlantStatus>();
    }

    private void OnMouseDown()
    {
        if (status == null)
            return;

        if (PatientManager.Instance != null
            && !PatientManager.Instance.PatientReady)
        {
            return;
        }

        string dialogue = status.GetDialogue();

        if (!string.IsNullOrEmpty(dialogue))
            TalkManager.Instance?.Show(dialogue);
    }
}
