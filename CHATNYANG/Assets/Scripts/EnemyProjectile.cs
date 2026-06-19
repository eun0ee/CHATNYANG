using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifeTime = 4f;

    private void Start()
    {
        // 일정 시간이 지나면 화면 밖으로 날아간 투사체 자동 삭제
        Destroy(gameObject, lifeTime);

        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // 플레이어 피격 처리 (기본 데미지)
                playerStats.TakeDamage(damage);

                // TODO: 속도 감소 디버프가 필요하다면 추후 이곳에 코루틴 로직 추가 가능
            }

            // 맞추면 즉시 사라짐
            Destroy(gameObject);
        }
    }
}