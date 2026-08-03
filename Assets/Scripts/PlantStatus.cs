using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlantStatus : MonoBehaviour
{
    public List<string> requiredTreatments = new List<string>();
    //모든 식물이 가진 상태
    [Header("Status")]

    [Header("Sprites")]

    public Sprite normalSprite;
    public Sprite deadSprite;
    SpriteRenderer sr;
    public float hp = 100;

    public float humidity = 50;

    public float temperature = 25;

    public float boredom = 30;

    public bool diagnosed;

    public bool treated;

    public bool isDead;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        normalSprite = sr.sprite;
        
        hp = Random.Range(60f, 100f);
        humidity = Random.Range(40f, 100f);
        GenerateRandomSymptoms();
    }
    public void GenerateRandomSymptoms()
    {
        requiredTreatments.Clear();

        requiredTreatments.Add("물 주기");
        requiredTreatments.Add("음악 들려주기");
    }
    public IEnumerator ShockEffect()
    {
        for (int i = 0; i < 6; i++)
        {
            sr.enabled = false;

            yield return new WaitForSeconds(0.08f);

            sr.enabled = true;

            yield return new WaitForSeconds(0.08f);
        }
    }

    public void Revive()
    {
        isDead = false;

        hp = 40;

        sr.sprite = normalSprite;

        currentMessage = "살아났어요!";
    }
    
    void Update()
    {
        
    }
    public string currentMessage =
        "몸이 으슬으슬해요.";
    

    public List<string> GetTalks()
    {
        List<string> talks = new List<string>();

        // HP
        if (hp < 20)
        {
            talks.Add("몸에 힘이 하나도 없어요...");
            talks.Add("기운이 없어요...");
        }
        else if (hp > 80)
        {
            talks.Add("너무 활력이 넘쳐요!");
            talks.Add("에너지가 넘쳐요!");
        }

        // Humidity
        if (humidity < 20)
        {
            talks.Add("목 말라요...");
            talks.Add("물 한 잔만 주세요...");
        }
        else if (humidity > 80)
        {
            talks.Add("더는 못 마시겠어요...");
            talks.Add("배가 물로 가득 찼어요...");
        }

        // Temperature
        if (temperature < 20)
        {
            talks.Add("으으... 너무 추워요.");
            talks.Add("따뜻한 곳으로 가고 싶어요.");
        }
        else if (temperature > 80)
        {
            talks.Add("여기 너무 덥지 않나요?");
            talks.Add("잎이 타버릴 것 같아요...");
        }

        // Boredom
        if (boredom > 80)
        {
            talks.Add("심심해서 죽겠어요...");
            talks.Add("노래 좀 들려주세요!");
        }

        // 정상 상태
        if (talks.Count == 0)
        {
            talks.Add("오늘은 기분이 좋아요!");
            talks.Add("햇빛이 참 좋네요.");
            talks.Add("흙 냄새가 좋아요.");
            talks.Add("오늘도 열심히 자라는 중이에요!");
        }

        // 잡담(항상 추가)
        talks.Add("벌이 놀러 왔어요.");
        talks.Add("오늘 날씨가 좋네요.");
        talks.Add("잎이 반짝반짝해요.");
        talks.Add("꽃을 피우고 싶어요.");

        return talks;
    }
    
}
