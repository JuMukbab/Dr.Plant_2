using TMPro;
using UnityEngine;


public class PatientReview : MonoBehaviour
{
    public GameObject reviewObject;

    public TMP_Text reviewText;

    public void ShowReview(string text)
    {

        reviewObject.SetActive(true);

        reviewText.text = text;

        // reviewText.color = Color.red;
        reviewText.fontSize = 10;
    }

    public void HideReview()
    {
        reviewObject.SetActive(false);
    }
}