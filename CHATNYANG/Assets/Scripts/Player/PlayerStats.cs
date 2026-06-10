using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHp = 100f;
    public float currentHp;
    public float moveSpeed = 5f;
    public float armor = 0f;
    public float recovery = 0f;

    [Header("Invincibility")]
    public float invincibilityDuration = 1.5f;

    private float _invincibleTimer = 0f;
    public bool IsInvincible => _invincibleTimer > 0f;
    private SpriteRenderer _spriteRenderer;

    // 이벤트 선언 추가
    public event System.Action<float, float> OnHpChanged; // (currentHp, maxHp)

    // �ν����� â�� ������� ���� �Ҵ��� �� �ְ� ����
    [SerializeField] private PlayerController _controller;

    private void Awake()
    {
        currentHp = maxHp;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Null üũ �� ����
        if (_controller != null)
        {
            _controller.MoveSpeed = moveSpeed;
        }
        else
        {
            Debug.LogError("Controller is missing");
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsInvincible) return;

        float finalDamage = Mathf.Max(0f, amount - armor);
        currentHp -= finalDamage;
        OnHpChanged?.Invoke(currentHp, maxHp);

        _invincibleTimer = invincibilityDuration;
        StartCoroutine(InvincibilityFlash()); // ← 추가

        if (currentHp <= 0f)
        {
            currentHp = 0f;
            Die();
        }
    }

    private IEnumerator InvincibilityFlash()
    {
        float flashInterval = 0.1f; // 깜빡임 간격
        Color original = _spriteRenderer.color;
        Color transparent = new Color(original.r, original.g, original.b, 0.3f);

        while (IsInvincible)
        {
            _spriteRenderer.color = transparent;
            yield return new WaitForSeconds(flashInterval);
            _spriteRenderer.color = original;
            yield return new WaitForSeconds(flashInterval);
        }

        // 무적 종료 후 원래 색으로 복구
        _spriteRenderer.color = original;
    }

    private void Update()
    {
        if (_invincibleTimer > 0f)
            _invincibleTimer -= Time.deltaTime;

        if (recovery > 0f)
        {
            currentHp = Mathf.Min(currentHp + recovery * Time.deltaTime, maxHp);
            OnHpChanged?.Invoke(currentHp, maxHp); // ← 회복할 때도 갱신
        }
    }

    private void Die()
    {
        Debug.Log("Player Dead");
        HUDManager.Instance?.ShowGameOver();
    }
}