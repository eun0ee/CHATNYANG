using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class BentoniteArea : MonoBehaviour
{
    private float slowFactor;
    private float duration;
    private List<Collider2D> affectedEnemies = new List<Collider2D>();

    public void Initialize(WeaponData data)
    {
        slowFactor = data.slowFactor;
        duration = data.areaDuration;

        // 데이터에 맞게 장판 크기 조절
        transform.localScale = new Vector3(data.aoeRadius, data.aoeRadius, 1f);

        // 일정 시간 뒤 장판 자동 소멸
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
        // 장판이 사라질 때 묶여있던 적들 속도 풀어주기
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