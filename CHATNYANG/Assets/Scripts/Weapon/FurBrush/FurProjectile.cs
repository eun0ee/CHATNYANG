using UnityEngine;
using System.Collections.Generic; // HashSet을 사용하기 위해 추가

[RequireComponent(typeof(Rigidbody2D))]
public class FurProjectile : MonoBehaviour
{
    private float damage;
    private Rigidbody2D rb;
    private float lifeTime = 4f;

    // 이미 타격한 적을 기억하여 중복 타격을 방지하는 리스트
    private HashSet<EnemyStats> hitEnemies = new HashSet<EnemyStats>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 물리적 밀림 현상 방지를 위해 키네마틱으로 설정
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Initialize(Vector2 direction, WeaponData data, WeaponRarity rarity = WeaponRarity.Normal, int upgradeLevel = 0)
    {
        WeaponStatValues stats = data.GetStats(rarity, upgradeLevel);

        if (stats == null)
        {
            Debug.LogWarning("[FurProjectile] Stats not found. Using Normal 0 stats.");
            stats = data.GetStats(WeaponRarity.Normal, 0);
        }

        if (stats != null)
        {
            damage = stats.damage;
            rb.velocity = direction * stats.projectileSpeed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifeTime);

        // 스폰 즉시 겹쳐있는 적을 수동으로 검사하여 가장 가까운 적이 맞지 않는 버그 해결
        CheckInitialOverlap();
    }

    private void CheckInitialOverlap()
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider == null) return;

        List<Collider2D> overlappedColliders = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;

        // 생성된 순간 내 콜라이더 안에 이미 들어와 있는 모든 적을 찾아냅니다.
        Physics2D.OverlapCollider(myCollider, filter, overlappedColliders);

        foreach (var col in overlappedColliders)
        {
            HitEnemy(col);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HitEnemy(collision);
    }

    // 데미지 처리 및 관통 로직을 하나로 묶은 함수
    private void HitEnemy(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyStats enemy = collision.GetComponent<EnemyStats>();

            // 아직 때리지 않은 적일 경우에만 데미지를 입힙니다.
            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                enemy.TakeDamage(damage);
                hitEnemies.Add(enemy); // 타격 기록 저장

                // 적을 관통할 때마다 데미지를 절반으로 감소시킵니다.
                damage /= 2f;

                // 만약 데미지가 1 이하로 너무 약해지면 더 이상 관통하지 않고 파괴하도록 설정할 수도 있습니다.
                // if (damage < 1f) Destroy(gameObject);
            }
        }
    }
}