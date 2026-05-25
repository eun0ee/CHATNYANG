using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHp = 100f;
    public float currentHp;
    public float moveSpeed = 5f;
    public float armor = 0f;
    public float recovery = 0f;  // 초당 HP 회복

    private PlayerController _controller;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        currentHp = maxHp;
    }

    private void Start()
    {
        _controller.MoveSpeed = moveSpeed;
    }

    private void Update()
    {
        // 자연 회복
        if (recovery > 0f)
            currentHp = Mathf.Min(currentHp + recovery * Time.deltaTime, maxHp);
    }

    public void TakeDamage(float amount)
    {
        float finalDamage = Mathf.Max(0f, amount - armor);
        currentHp -= finalDamage;

        if (currentHp <= 0f)
        {
            currentHp = 0f;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Dead");
        // 나중에 GameManager.Instance.GameOver() 연결
    }
}