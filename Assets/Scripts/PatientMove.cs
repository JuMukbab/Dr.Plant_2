using UnityEngine;
using System;

public class PatientMove : MonoBehaviour
{
    public float speed = 2f;

    Vector3 target;
    bool moving;

    public Action onArrive;

    public void MoveTo(Vector3 pos)
    {
        target = pos;
        moving = true;
    }

    void Update()
    {
        if (!moving)
            return;

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            moving = false;
            onArrive?.Invoke();
        }
    }
}