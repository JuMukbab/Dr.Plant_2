using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static GameManager Instance;

    public int money;
    public int treatedPlant;

    public bool gameOver;

    void Awake()
    {
        Instance = this;
    }

    public void AddMoney(int value)
    {
        money += value;
    }

    public void PlantSaved()
    {
        treatedPlant++;
    }

    public void GameOver()
    {
        gameOver = true;
    }
}