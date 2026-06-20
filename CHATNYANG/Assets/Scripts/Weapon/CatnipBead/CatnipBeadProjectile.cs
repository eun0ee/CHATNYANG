using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class CatnipBeadProjectile : MonoBehaviour
{
    private float damage;
    private int maxBounce;
    private int currentBounce;
    private Rigidbody2D rb;
    private Collider2D myCollider;

    private float lifeTime = 10f;

    private float moveSpeed;
    private Vector2 lastVelocity;

    // 게임 내에 존재하는 모든 구슬의 콜라이더를 추적하여 서로 충돌하지 않게 함
    public static List<Collider2D> activeBeads = new List<Collider2D>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        rb.mass = 0.0001f;

        // 스폰될 때, 이미 날아가고 있는 다른 구슬들과의 물리적 충돌을 완전히 무시하도록 설정
        foreach (Collider2D otherBead in activeBeads)
        {
            if (otherBead != null)
            {
                Physics2D.IgnoreCollision(myCollider, otherBead);
            }
        }

        // 나 자신도 리스트에 등록
        activeBeads.Add(myCollider);
    }

    private void OnDestroy()
    {
        // 파괴될 때 리스트에서 제거
        if (activeBeads.Contains(myCollider))
        {
            activeBeads.Remove(myCollider);
        }
    }

    public void Initialize(Vector2 direction, WeaponData data, WeaponRarity rarity = WeaponRarity.Normal, int upgradeLevel = 0)
    {
        WeaponStatValues stats = data.GetStats(rarity, upgradeLevel);

        if (stats == null)
        {
            Debug.LogWarning("[CatnipBeadProjectile] Stats not found. Using Normal 0 stats.");
            stats = data.GetStats(WeaponRarity.Normal, 0);
        }

        if (stats != null)
        {
            damage = stats.damage;
            maxBounce = stats.bounceCount;
            moveSpeed = stats.projectileSpeed;
            rb.velocity = direction * moveSpeed;
        }

        currentBounce = 0;
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        lastVelocity = rb.velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyStats enemy = collision.gameObject.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        if (currentBounce < maxBounce)
        {
            currentBounce++;
            Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, collision.contacts[0].normal);
            rb.velocity = reflectDir * Mathf.Max(lastVelocity.magnitude, moveSpeed);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}