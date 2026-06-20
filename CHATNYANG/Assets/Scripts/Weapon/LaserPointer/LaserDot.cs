using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class LaserDot : MonoBehaviour
{
    private float damage;
    private float tickRate = 0.15f;
    private float tickTimer = 0f;
    private float moveSpeed;
    private float radius;

    private Transform playerTarget;
    private Vector2 randomTargetPos;

    private List<Collider2D> enemiesInRange = new List<Collider2D>();
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Initialize(Transform player, WeaponData data, WeaponRarity rarity = WeaponRarity.Normal, int upgradeLevel = 0)
    {
        playerTarget = player;

        WeaponStatValues stats = data.GetStats(rarity, upgradeLevel);

        if (stats == null)
        {
            Debug.LogWarning("[LaserDot] Stats not found. Using Normal 0 stats.");
            stats = data.GetStats(WeaponRarity.Normal, 0);
        }

        if (stats != null)
        {
            damage = stats.damage;
            moveSpeed = stats.projectileSpeed;
            radius = stats.aoeRadius;

            // 크기를 강제로 키우던 transform.localScale 덮어쓰기 코드를 삭제했습니다.
            // 프리팹에 설정된 0.2 크기가 그대로 유지됩니다.

            Destroy(gameObject, stats.attackCooldown);
        }

        SetNewRandomTarget();
    }

    private void FixedUpdate()
    {
        if (playerTarget == null) return;

        Vector2 nextPos = Vector2.MoveTowards(rb.position, randomTargetPos, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPos);

        if (Vector2.Distance(rb.position, randomTargetPos) < 0.1f)
        {
            SetNewRandomTarget();
        }
    }

    private void Update()
    {
        if (playerTarget == null) return;

        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            ApplyTickDamage();
            tickTimer = tickRate;
        }
    }

    private void SetNewRandomTarget()
    {
        randomTargetPos = (Vector2)playerTarget.position + Random.insideUnitCircle * radius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!enemiesInRange.Contains(other))
            {
                enemiesInRange.Add(other);

                EnemyStats enemy = other.GetComponent<EnemyStats>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (enemiesInRange.Contains(other))
            {
                enemiesInRange.Remove(other);
            }
        }
    }

    private void ApplyTickDamage()
    {
        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            if (enemiesInRange[i] == null || !enemiesInRange[i].gameObject.activeInHierarchy)
            {
                enemiesInRange.RemoveAt(i);
                continue;
            }

            EnemyStats enemy = enemiesInRange[i].GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}