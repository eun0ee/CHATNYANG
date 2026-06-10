using UnityEngine;

public class EnemyContact : MonoBehaviour
{
    private EnemyStats _enemyStats;

    [SerializeField] private float contactRadius = 0.5f;
    [SerializeField] private LayerMask playerLayer; // 인스펙터에서 Player 레이어 선택

    private void Awake()
    {
        _enemyStats = GetComponent<EnemyStats>();
    }

    private void FixedUpdate()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position, contactRadius, playerLayer);

            Debug.Log($"[EnemyContact] OverlapCircle 결과: {(hit != null ? hit.gameObject.name : "null")}");

        if (hit != null && hit.TryGetComponent<PlayerStats>(out var playerStats))
        {
            Debug.Log($"플레이어 감지 / 데미지: {_enemyStats.Data.damage}");
            playerStats.TakeDamage(_enemyStats.Data.damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, contactRadius);
    }
}