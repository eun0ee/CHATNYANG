using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHp = 100f;
    public float currentHp;
    public float moveSpeed = 5f;
    public float armor = 0f;
    public float recovery = 0f;

    // 인스펙터 창에 노출시켜 직접 할당할 수 있게 변경
    [SerializeField] private PlayerController _controller;

    private void Awake()
    {
        currentHp = maxHp;
    }

    private void Start()
    {
        // Null 체크 후 실행
        if (_controller != null)
        {
            _controller.MoveSpeed = moveSpeed;
        }
        else
        {
            Debug.LogError("Controller is missing");
        }
    }

    private void Update()
    {
        if (recovery > 0f)
        {
            currentHp = Mathf.Min(currentHp + recovery * Time.deltaTime, maxHp);
        }
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
    }
}