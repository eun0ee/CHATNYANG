using UnityEngine;

public class CatnipBeadWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        if (weaponData.projectilePrefab == null) return;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;

        // 발사체 생성 및 초기화
        for (int i = 0; i < weaponData.projectileCount; i++)
        {
            GameObject bead = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);
            CatnipBeadProjectile projectileScript = bead.GetComponent<CatnipBeadProjectile>();

            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, weaponData);
            }
        }
    }

    // 팀원 코드 수정 없이 유니티 기본 태그 검색을 활용한 탐색
    private Transform FindNearestEnemy()
    {
        // 씬에 있는 Enemy 태그를 가진 모든 오브젝트를 찾음
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }
}