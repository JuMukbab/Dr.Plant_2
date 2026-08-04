using UnityEngine;

public class Thermometer : ToolBase
{
    //온도계
    public override void Use(PlantStatus target)
    {
        Debug.Log(target.temperature);

        Debug.Log(target.humidity);
    }
}
