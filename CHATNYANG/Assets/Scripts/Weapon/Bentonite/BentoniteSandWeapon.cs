using UnityEngine;

public class BentoniteSandWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        if (weaponData.projectilePrefab == null) return;

        // 임시로 노말 0강 스탯을 가져옴. 향후 WeaponBase에서 등급 변수를 추가하면 그것을 사용하도록 수정 요망.
        WeaponStatValues stats = weaponData.GetStats(WeaponRarity.Normal, 0);
        if (stats == null) return;

        Transform target = FindNearestEnemy();

        Vector2 targetPos = target != null ? (Vector2)target.position : (Vector2)transform.position + Random.insideUnitCircle * 5f;

        for (int i = 0; i < stats.projectileCount; i++)
        {
            GameObject sandBag = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);
            BentoniteProjectile projectileScript = sandBag.GetComponent<BentoniteProjectile>();

            if (projectileScript != null)
            {
                // 인자를 넘겨주지 않으면 자동으로 노말 0강으로 세팅됨
                projectileScript.Initialize(targetPos, weaponData);
            }
        }
    }
}