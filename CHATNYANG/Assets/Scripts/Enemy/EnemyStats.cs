using UnityEngine;

[System.Serializable]
public class EnemyStatData
{
    public string enemyName = "Enemy";
    public float maxHp = 30f;
    public float moveSpeed = 2f;
    public float damage = 10f;
    public float attackCooldown = 1f;
    public int expReward = 10;
}

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private EnemyStatData data;

    public float CurrentHp { get; private set; }
    public EnemyStatData Data => data;

    public event System.Action OnDeath;
    public event System.Action<float> OnDamaged;

    private void Awake()
    {
        CurrentHp = data.maxHp;
    }

    public void TakeDamage(float amount)
{
    Debug.Log($"[TakeDamage] 호출됨 / amount: {amount} / CurrentHp: {CurrentHp}");
    CurrentHp -= amount;
    OnDamaged?.Invoke(amount);

    if (CurrentHp <= 0f)
        Die();
}

    private void Die()
    {
        Debug.Log($"[EnemyStats] {gameObject.name} Dead / expReward: {data.expReward}");

        // HUDManager의 싱글톤 인스턴스를 통해 킬 수를 1 올립니다.
        HUDManager.Instance?.AddKill();

        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}