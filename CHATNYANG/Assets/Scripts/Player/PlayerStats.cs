using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHp = 100f;
    public float currentHp;
    public float moveSpeed = 5f;
    public float armor = 0f;
    public float recovery = 0f;

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
        Application.Quit();
        Debug.Log("Player Dead");
    }
}