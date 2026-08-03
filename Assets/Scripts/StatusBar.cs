using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    public static StatusBar Instance;

    public Image hpBar;
    public Image humidityBar;
    public Image tempBar;
    public Image boredomBar;

    private PlantStatus target;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (target == null)
            return;

        hpBar.fillAmount = target.hp / 100f;
        humidityBar.fillAmount = target.humidity / 100f;
        tempBar.fillAmount = target.temperature / 100f;
        boredomBar.fillAmount = target.boredom / 100f;
    }

    public void SetTarget(PlantStatus newTarget)
    {
        target = newTarget;

        hpBar.fillAmount = target.hp / 100f;
        humidityBar.fillAmount = target.humidity / 100f;
        tempBar.fillAmount = target.temperature / 100f;
        boredomBar.fillAmount = target.boredom / 100f;
    }
}