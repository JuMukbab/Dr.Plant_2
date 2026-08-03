using UnityEngine;

public class WaterButton : MonoBehaviour
{
    
    public PlantStatus target;

    //물 주기
    public void Water()
    {
        target.humidity += 30;

        if(target.humidity > 100)
            target.humidity = 100;
    }
    //클래식 음악
    public void Music()
    {
        target.boredom -= 40;

        if(target.boredom < 0)
            target.boredom = 0;
    }
    //선풍기
    public void Fan()
    {
        target.temperature -= 5;
    }

    //CPR
    public void CPR()
    {
        if(target.hp > 20)
            return;

        if(Random.value < 0.7f)
        {
            target.hp = 40;
        }
        else
        {
            target.hp = 0;
        }
    }

    //커피(콩: 즉사 / 꽃: HP+10 / 선인장 HP -30)
    public void Coffee()
    {
        target.hp -= 50;
    }
}
