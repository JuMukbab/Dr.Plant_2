using UnityEngine;

public class Translator : ToolBase
{
    //식물 번역기
    public override void Use(PlantStatus target)
    {
        if(target.temperature < 15)
            Debug.Log("몸이 으슬으슬해요... 으...");

        else if(target.humidity < 20)
            Debug.Log("목이 말라요.");

        else if(target.boredom > 70)
            Debug.Log("심심해요...............");

        else
            Debug.Log("괜찮아요!");
    }
}
