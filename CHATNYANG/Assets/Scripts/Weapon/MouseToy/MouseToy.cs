using UnityEngine;

public class MouseToy : MonoBehaviour
{
    private WeaponData data;
    private Transform targetTransform;

    private float speed;
    private float explosionRadius;
    private float damage;
    private float areaDuration;

    [Header("Zigzag Settings")]
    [SerializeField] private float zigzagFrequency = 15f;
    [SerializeField] private float zigzagMagnitude = 4f;

    private float aliveTimer = 0f;

    public void Initialize(WeaponData weaponData, Transform target, WeaponRarity rarity = WeaponRarity.Normal, int upgradeLevel = 0)
    {
        data = weaponData;
        targetTransform = target;

        WeaponStatValues stats = data.GetStats(rarity, upgradeLevel);

        if (stats == null)
        {
            Debug.LogWarning("[MouseToy] Stats not found. Using Normal 0 stats.");
            stats = data.GetStats(WeaponRarity.Normal, 0);
        }

        if (stats != null)
        {
            speed = stats.projectileSpeed;
            explosionRadius = stats.aoeRadius;
            damage = stats.damage;
            areaDuration = stats.areaDuration;
        }

        Destroy(gameObject, 7f);
    }

    private void Update()
    {
        aliveTimer += Time.deltaTime;

        if (targetTransform == null || !targetTransform.gameObject.activeInHierarchy)
        {
            targetTransform = FindHighestHpEnemy();
            if (targetTransform == null)
            {
                Explode();
                return;
            }
        }

        Vector2 baseDirection = ((Vector2)targetTransform.position - (Vector2)transform.position).normalized;
        Vector2 perpendicular = new Vector2(-baseDirection.y, baseDirection.x);

        float zigzagOffset = Mathf.Sin(aliveTimer * zigzagFrequency) * zigzagMagnitude;
        Vector2 finalMovement = (baseDirection * speed) + (perpendicular * zigzagOffset);

        transform.Translate(finalMovement * Time.deltaTime, Space.World);

        if (finalMovement != Vector2.zero)
        {
            float angle = Mathf.Atan2(finalMovement.y, finalMovement.x) * Mathf.Rad2Deg;

            // 1. 이미지가 왼쪽을 보고 있으므로 180도를 더해 시선을 이동 방향과 맞춰줍니다.
            transform.rotation = Quaternion.Euler(0, 0, angle + 180f);

            // 2. 오른쪽으로 갈 때 이미지가 위아래로 뒤집어지는 현상을 방지합니다.
            Vector3 currentScale = transform.localScale;
            if (finalMovement.x > 0)
            {
                // 오른쪽으로 이동 중: Y 스케일을 마이너스로 반전시켜 똑바로 보이게 함
                currentScale.y = -Mathf.Abs(currentScale.y);
            }
            else if (finalMovement.x < 0)
            {
                // 왼쪽으로 이동 중: Y 스케일을 다시 원래대로 원복
                currentScale.y = Mathf.Abs(currentScale.y);
            }
            transform.localScale = currentScale;
        }

        if (Vector2.Distance(transform.position, targetTransform.position) < 0.2f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyStats enemy = col.GetComponent<EnemyStats>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        if (data.areaPrefab != null)
        {
            GameObject effect = Instantiate(data.areaPrefab, transform.position, Quaternion.identity);

            float destroyTime = areaDuration > 0f ? areaDuration : 0.2f;
            Destroy(effect, destroyTime);
        }

        Destroy(gameObject);
    }

    private Transform FindHighestHpEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform highestHpEnemy = null;
        float maxHp = -1f;

        foreach (GameObject enemy in enemies)
        {
            EnemyStats stats = enemy.GetComponent<EnemyStats>();
            if (stats != null && stats.CurrentHp > maxHp)
            {
                maxHp = stats.CurrentHp;
                highestHpEnemy = enemy.transform;
            }
        }
        return highestHpEnemy;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}