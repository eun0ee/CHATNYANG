using UnityEngine;

public class LaserPointerWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        if (weaponData.projectilePrefab == null) return;

        for (int i = 0; i < weaponData.projectileCount; i++)
        {
            GameObject laser = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);

            LaserDot projectileScript = laser.GetComponent<LaserDot>();
            if (projectileScript != null)
            {
                // 레이저가 플레이어를 중심으로 맴돌 수 있도록 플레이어의 Transform을 넘겨줌
                projectileScript.Initialize(transform, weaponData);
            }
        }
    }
}