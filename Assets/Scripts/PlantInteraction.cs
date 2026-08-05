using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlantInteraction : MonoBehaviour
{
    
    PlantStatus status;

    void Start()
    {
        status = GetComponent<PlantStatus>();
    }

    void Examine()
    {
        List<string> talks = status.GetTalks();

        string talk =
            talks[Random.Range(0, talks.Count)];

        TalkManager.Instance.Show(talk);
    }
    void OnMouseDown()
    {
        Examine();
    }
}