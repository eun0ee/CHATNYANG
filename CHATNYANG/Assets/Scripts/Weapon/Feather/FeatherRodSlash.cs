using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class FeatherRodSlash : MonoBehaviour
{
    private float damage;
    private float knockbackPower = 2.5f;
    private Vector2 sourcePosition;

    public void Initialize(WeaponData data, Vector2 sourcePos, WeaponRarity rarity = WeaponRarity.Normal, int upgradeLevel = 0)
    {
        sourcePosition = sourcePos;

        WeaponStatValues stats = data.GetStats(rarity, upgradeLevel);

        if (stats == null)
        {
            Debug.LogWarning("[FeatherRodSlash] Stats not found. Using Normal 0 stats.");
            stats = data.GetStats(WeaponRarity.Normal, 0);
        }

        if (stats != null)
        {
            damage = stats.damage;
            transform.localScale = Vector3.one * stats.aoeRadius;
        }

        Destroy(gameObject, 0.15f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyStats enemy = other.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                Vector2 knockbackDir = ((Vector2)other.transform.position - sourcePosition).normalized;

                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    StartCoroutine(ApplyKnockback(rb, knockbackDir));
                }
            }
        }
    }

    private IEnumerator ApplyKnockback(Rigidbody2D rb, Vector2 direction)
    {
        rb.AddForce(direction * knockbackPower, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.1f);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
}