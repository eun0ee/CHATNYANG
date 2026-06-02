using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class BentoniteArea : MonoBehaviour
{
    private float slowFactor;
    private float duration;
    private List<Collider2D> affectedEnemies = new List<Collider2D>();

    // 확장성을 위해 매개변수 추가
    public void Initialize(WeaponData data, WeaponRarity rarity = WeaponRarity.Normal, int upgradeLevel = 0)
    {
        WeaponStatValues stats = data.GetStats(rarity, upgradeLevel);

        if (stats == null)
        {
            Debug.LogWarning("[BentoniteArea] Stats not found. Using Normal 0 stats.");
            stats = data.GetStats(WeaponRarity.Normal, 0);
        }

        if (stats != null)
        {
            slowFactor = stats.slowFactor;
            duration = stats.areaDuration;
            transform.localScale = new Vector3(stats.aoeRadius, stats.aoeRadius, 1f);
        }

        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            affectedEnemies.Add(other);
            ApplySlow(other.gameObject, true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            affectedEnemies.Remove(other);
            ApplySlow(other.gameObject, false);
        }
    }

    private void OnDestroy()
    {
        foreach (Collider2D enemy in affectedEnemies)
        {
            if (enemy != null)
            {
                ApplySlow(enemy.gameObject, false);
            }
        }
    }

    private void ApplySlow(GameObject enemyObj, bool isSlowed)
    {
        EnemyAI ai = enemyObj.GetComponent<EnemyAI>();
        if (ai != null)
            ai.SetSpeedMultiplier(isSlowed ? slowFactor : 1f);
    }
}