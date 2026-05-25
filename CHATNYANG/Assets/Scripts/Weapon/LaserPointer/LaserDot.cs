using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class LaserDot : MonoBehaviour
{
    private float damage;
    private float tickRate = 0.25f; // 대미지가 들어가는 주기 (0.25초마다 타격)
    private float tickTimer = 0f;
    private float moveSpeed;
    private float radius;

    private Transform playerTarget;
    private Vector2 randomTargetPos;

    // 레이저 범위 안에 있는 적들을 추적하기 위한 리스트
    private List<Collider2D> enemiesInRange = new List<Collider2D>();

    public void Initialize(Transform player, WeaponData data)
    {
        playerTarget = player;
        damage = data.damage;
        moveSpeed = data.projectileSpeed;
        radius = data.aoeRadius;

        // 무기 쿨타임만큼 생존한 뒤 파괴되어 새로운 레이저로 갱신되도록 함
        Destroy(gameObject, data.attackCooldown);

        SetNewRandomTarget();
    }

    private void Update()
    {
        if (playerTarget == null) return;

        // 랜덤하게 찍힌 목표 좌표를 향해 미친듯이 이동
        transform.position = Vector2.MoveTowards(transform.position, randomTargetPos, moveSpeed * Time.deltaTime);

        // 목표 지점에 거의 도달하면 즉시 다음 랜덤 좌표 생성
        if (Vector2.Distance(transform.position, randomTargetPos) < 0.1f)
        {
            SetNewRandomTarget();
        }

        // 틱 대미지 타이머 계산
        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            ApplyTickDamage();
            tickTimer = tickRate;
        }
    }

    // 플레이어 주변(radius 반경 내)의 무작위 위치를 다음 목표로 설정
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
        // 리스트를 역순으로 순회하며 적이 이미 죽어서 파괴되었는지 검사 후 대미지 적용
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