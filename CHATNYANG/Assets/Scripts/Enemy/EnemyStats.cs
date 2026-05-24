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
        CurrentHp -= amount;
        OnDamaged?.Invoke(amount);

        if (CurrentHp <= 0f)
            Die();
    }

    private void Die()
    {
        OnDeath?.Invoke();
        // 풀링 쓸 거면 여기서 Destroy 대신 반환
        Destroy(gameObject);
    }
}