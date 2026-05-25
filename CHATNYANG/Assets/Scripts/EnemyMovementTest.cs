using UnityEngine;

public class EnemyMovementTest : MonoBehaviour
{
    // 이동 속도
    public float speed = 2f;
    private Transform targetPlayer;

    private void Start()
    {
        // 씬에서 플레이어 태그를 가진 오브젝트 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            targetPlayer = playerObj.transform;
        }
    }

    private void Update()
    {
        // 플레이어가 존재하면 그 방향으로 이동
        if (targetPlayer != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, speed * Time.deltaTime);
        }
    }
}