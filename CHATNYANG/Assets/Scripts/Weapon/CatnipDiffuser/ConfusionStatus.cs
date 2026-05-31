using UnityEngine;

public class ConfusionStatus : MonoBehaviour
{
    private float duration;
    private float timer;
    private float reverseSpeed;
    private Transform playerTransform;

    public void ActivateStatus(float time, float baseSpeed)
    {
        duration = time;
        timer = 0f;
        reverseSpeed = baseSpeed * 0.5f;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(this);
            return;
        }

        if (playerTransform != null)
        {
            Vector2 direction = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
            transform.Translate(direction * reverseSpeed * Time.deltaTime, Space.World);
        }
    }
}