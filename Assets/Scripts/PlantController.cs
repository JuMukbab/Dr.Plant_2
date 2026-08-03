using UnityEngine;

public class PlantController : MonoBehaviour
{
    PlantStatus status;

    void Start()
    {
        status = GetComponent<PlantStatus>();
    }

    void Update()
    {
        if (status == null)
        {
            Debug.LogError("status가 null");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance가 null");
        }

        if (status.isDead)
            return;

        if (status.hp <= 0)
        {
            status.hp = 0;
            status.isDead = true;

            GameManager.Instance.GameOver();
        }
    }
}
