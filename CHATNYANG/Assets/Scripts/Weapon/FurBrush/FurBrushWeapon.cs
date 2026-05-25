using UnityEngine;

public class FurBrushWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        if (weaponData.projectilePrefab == null) return;

        // 발사할 방향의 각도 간격 계산 (예: 4개면 90도, 8개면 45도)
        float angleStep = 360f / weaponData.projectileCount;

        for (int i = 0; i < weaponData.projectileCount; i++)
        {
            // 현재 투사체의 발사 각도 계산
            float currentAngle = i * angleStep;

            // 각도를 라디안으로 변환하여 방향 벡터(X, Y) 추출
            float radian = currentAngle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;

            // 투사체 생성
            GameObject fur = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);

            // 방향과 데이터를 전달하여 초기화
            FurProjectile projectileScript = fur.GetComponent<FurProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, weaponData);
            }
        }
    }
}