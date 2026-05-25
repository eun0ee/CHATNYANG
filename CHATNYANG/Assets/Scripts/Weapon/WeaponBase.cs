using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected WeaponData weaponData;
    protected float currentCooldown;

    protected virtual void Update()
    {
        currentCooldown -= Time.deltaTime;
        if (currentCooldown <= 0f)
        {
            ExecuteAttack();
            currentCooldown = weaponData.attackCooldown;
        }
    }

    // 개별 무기들의 공격 로직
    protected abstract void ExecuteAttack();
}