using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyAI : MonoBehaviour
{
    private Rigidbody2D _rb;
    private EnemyStats _stats;
    private Transform _target;

    private float _attackTimer;
    private bool _isDead;
    private float _speedMultiplier = 1f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        _stats = GetComponent<EnemyStats>();
        _stats.OnDeath += () => _isDead = true;
    }

    private void Start()
    {
        // 씬에서 Player 태그로 자동 탐색
        var player = GameObject.FindWithTag("Player");
        if (player != null) _target = player.transform;
    }

    private void FixedUpdate()
    {
        if (_isDead || _target == null) return;
        ChaseTarget();
    }

    private void Update()
    {
        if (_isDead || _target == null) return;
        _attackTimer -= Time.deltaTime;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }

    private void ChaseTarget()
    {
        Vector2 direction = (_target.position - transform.position).normalized;
        _rb.MovePosition(_rb.position + direction * (_stats.Data.moveSpeed * _speedMultiplier * Time.fixedDeltaTime));
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (_attackTimer > 0f) return;

        // PlayerStats에 데미지 전달
        var playerStats = other.GetComponent<PlayerStats>();
        playerStats?.TakeDamage(_stats.Data.damage);

        _attackTimer = _stats.Data.attackCooldown;
    }

    // 외부(무기 등)에서 타겟 교체 가능
    public void SetTarget(Transform newTarget) => _target = newTarget;
}