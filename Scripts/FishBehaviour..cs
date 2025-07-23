using System.Collections;
using UnityEngine;

public class FishBehaviour : MonoBehaviour
{
    public float idleSwimRadius = 0.1f;
    public float idleSpeed = 0.2f;

    private bool movingToTarget = false;
    private Vector3 target;
    private float approachSpeed;
    private System.Action onArrived;

    void Update()
    {
        if (movingToTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, approachSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                movingToTarget = false;
                onArrived?.Invoke();
            }
        }
        else
        {
            // Лёгкое покачивание, если не движется к цели
            Vector2 offset = Random.insideUnitCircle * idleSwimRadius;
            transform.position += (Vector3)offset * idleSpeed * Time.deltaTime;
        }
    }

    public void MoveTo(Vector3 targetPosition, float speed, System.Action callback)
    {
        target = targetPosition;
        approachSpeed = speed;
        onArrived = callback;
        movingToTarget = true;
    }
}
