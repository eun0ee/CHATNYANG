using UnityEngine;

public class BentoniteProjectile : MonoBehaviour
{
    private Vector2 targetPosition;
    private WeaponData data;
    private bool isInitialized = false;

    public void Initialize(Vector2 target, WeaponData weaponData)
    {
        targetPosition = target;
        data = weaponData;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        // 목표 지점을 향해 등속 이동 (포물선 연출은 추후 Z축이나 트위닝으로 추가 가능)
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, data.projectileSpeed * Time.deltaTime);

        // 목표 지점 도달 확인
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // 1. 범위 내 적들에게 강력한 한방 대미지
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, data.aoeRadius);
        foreach (Collider2D hit in hitEnemies)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyStats enemy = hit.GetComponent<EnemyStats>();
                if (enemy != null)
                {
                    enemy.TakeDamage(data.damage);
                }
            }
        }

        // 2. 장판(Area) 생성
        if (data.areaPrefab != null)
        {
            GameObject puddle = Instantiate(data.areaPrefab, transform.position, Quaternion.identity);
            BentoniteArea areaScript = puddle.GetComponent<BentoniteArea>();
            if (areaScript != null)
            {
                areaScript.Initialize(data);
            }
        }

        // 포대 파괴
        Destroy(gameObject);
    }
}