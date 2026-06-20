using UnityEngine;

public class ConfusionStatus : MonoBehaviour
{
    private float duration;
    private float timer;
    private float reverseSpeed;
    private Transform playerTransform;

    // 원래의 추적 AI를 제어하기 위한 변수
    private EnemyAI enemyAI;

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

        // 혼란 상태가 되면 적의 원래 추적 인공지능을 강제로 꺼버림
        if (enemyAI == null)
        {
            enemyAI = GetComponent<EnemyAI>();
        }

        if (enemyAI != null)
        {
            enemyAI.enabled = false;
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
            // 플레이어 반대 방향으로 깔끔하게 도망침
            Vector2 direction = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
            transform.Translate(direction * reverseSpeed * Time.deltaTime, Space.World);
        }
    }

    private void OnDestroy()
    {
        // 시간이 다 되어 혼란 스크립트가 파괴될 때 다시 추적 기능을 켜줌
        if (enemyAI != null)
        {
            enemyAI.enabled = true;
        }
    }
}