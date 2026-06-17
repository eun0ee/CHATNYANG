using UnityEngine;

public enum MovementPattern
{
    Chase,  // 직선 추적
    Charge, // 돌진
    Orbit   // 주위를 돔
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyAI : MonoBehaviour
{
    [Header("이동 패턴")]
    [SerializeField] private MovementPattern _pattern = MovementPattern.Chase;

    [Header("Charge 설정")]
    [SerializeField] private float _chargeTriggerRange = 4f;
    [SerializeField] private float _chargeSpeedMultiplier = 2.5f;
    [SerializeField] private float _chargeDuration = 0.6f;
    [SerializeField] private float _chargeCooldown = 1.5f;

    [Header("Orbit 설정")]
    [SerializeField] private float _orbitDistance = 3f;
    [SerializeField] private float _orbitSpeedMultiplier = 1f;
    [SerializeField] private bool _orbitClockwise = true;

    private Rigidbody2D _rb;
    private EnemyStats _stats;
    private Transform _target;

    private float _attackTimer;
    private bool _isDead;
    private float _speedMultiplier = 1f;

    private enum ChargeState { Approach, Dashing, Cooldown }
    private ChargeState _chargeState = ChargeState.Approach;
    private float _chargeStateTimer;
    private Vector2 _chargeDirection;

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

        switch (_pattern)
        {
            case MovementPattern.Chase:
                ChaseTarget();
                break;
            case MovementPattern.Charge:
                ChargeTarget();
                break;
            case MovementPattern.Orbit:
                OrbitTarget();
                break;
        }
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

    public void SetPattern(MovementPattern pattern)
    {
        _pattern = pattern;
        _chargeState = ChargeState.Approach; // 패턴 변경 시 상태 초기화
    }

    private void ChaseTarget()
    {
        Vector2 direction = (_target.position - transform.position).normalized;
        _rb.MovePosition(_rb.position + direction * (_stats.Data.moveSpeed * _speedMultiplier * Time.fixedDeltaTime));
    }

    private void ChargeTarget()
    {
        switch (_chargeState)
        {
            case ChargeState.Approach:
                float distance = Vector2.Distance(_target.position, transform.position);
                if (distance <= _chargeTriggerRange)
                {
                    _chargeDirection = (_target.position - transform.position).normalized;
                    _chargeState = ChargeState.Dashing;
                    _chargeStateTimer = _chargeDuration;
                }
                else
                {
                    ChaseTarget();
                }
                break;

            case ChargeState.Dashing:
                _rb.MovePosition(_rb.position + _chargeDirection * (_stats.Data.moveSpeed * _chargeSpeedMultiplier * _speedMultiplier * Time.fixedDeltaTime));
                _chargeStateTimer -= Time.fixedDeltaTime;
                if (_chargeStateTimer <= 0f)
                {
                    _chargeState = ChargeState.Cooldown;
                    _chargeStateTimer = _chargeCooldown;
                }
                break;

            case ChargeState.Cooldown:
                ChaseTarget(); // 쿨다운 중엔 천천히 추적만
                _chargeStateTimer -= Time.fixedDeltaTime;
                if (_chargeStateTimer <= 0f)
                    _chargeState = ChargeState.Approach;
                break;
        }
    }

    private void OrbitTarget()
    {
        Vector2 toTarget = _target.position - transform.position;
        float distance = toTarget.magnitude;
        Vector2 radialDir = toTarget.normalized;

        const float buffer = 0.3f;
        Vector2 moveDir;

        if (distance > _orbitDistance + buffer)
        {
            moveDir = radialDir; // 너무 멀면 접근
        }
        else if (distance < _orbitDistance - buffer)
        {
            moveDir = -radialDir; // 너무 가까우면 후퇴
        }
        else
        {
            Vector2 tangent = new Vector2(-radialDir.y, radialDir.x);
            moveDir = _orbitClockwise ? -tangent : tangent;
        }

        _rb.MovePosition(_rb.position + moveDir * (_stats.Data.moveSpeed * _orbitSpeedMultiplier * _speedMultiplier * Time.fixedDeltaTime));
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (_attackTimer > 0f) return;

        var playerStats = other.GetComponent<PlayerStats>();
        playerStats?.TakeDamage(_stats.Data.damage);

        _attackTimer = _stats.Data.attackCooldown;
    }

    public void SetTarget(Transform newTarget) => _target = newTarget;
}