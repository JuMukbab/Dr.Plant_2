using UnityEngine;
using TMPro;

public class SpeechBubble : MonoBehaviour
{
    public TMP_Text text;

    PlantStatus plant;

    void Start()
    {
        plant = FindFirstObjectByType<PlantStatus>();
    }

    void Update()
    {
        if (plant.temperature < 15)
            text.text = "추워요.";

        else if (plant.humidity < 30)
            text.text = "목말라요.";

        else if (plant.boredom > 70)
            text.text = "심심해요.";

        else
            text.text = "...";
    }
}
