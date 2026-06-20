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

    [Header("Charge 설정 (주사기)")]
    [SerializeField] private float _chargeTriggerRange = 4f;
    [SerializeField] private float _chargeSpeedMultiplier = 2.5f;
    [SerializeField] private float _chargeDuration = 0.6f;
    [SerializeField] private float _chargeCooldown = 1.5f;

    [Header("Orbit 설정 (분무기)")]
    [SerializeField] private float _orbitDistance = 3f;
    [SerializeField] private float _orbitSpeedMultiplier = 1f;
    [SerializeField] private bool _orbitClockwise = true;

    [Header("겹침 방지 (Anti-Clumping)")]
    [SerializeField] private bool useAvoidance = true;
    [SerializeField] private float avoidanceRadius = 1.0f; // 주변 적 감지 반경
    [SerializeField] private float avoidanceForce = 0.6f;  // 밀어내는 힘의 세기
    [SerializeField] private LayerMask enemyLayer;         // 적 레이어 지정

    [Header("기믹: 진공청소기 장판 (Slow)")]
    [SerializeField] private bool useVacuumArea = false;
    [SerializeField] private float vacuumTriggerRadius = 3f; // 이 반경 내에 플레이어가 들어오면 멈춰서 시전
    [SerializeField] private float vacuumPrepTime = 0.5f;    // 시전 준비 시간 (0.5초)
    [SerializeField] private float vacuumActiveTime = 2f;    // 장판 전개 시간 (2초)
    [SerializeField] private float vacuumCooldown = 3f;      // 스킬 쿨타임

    [Header("기믹: 분무기 투사체 발사")]
    [SerializeField] private bool useSprayShoot = false;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootCooldown = 3f;

    private float _shootTimer;
    private Rigidbody2D _rb;
    private EnemyStats _stats;
    private Transform _target;

    private float _attackTimer;
    private bool _isDead;
    private float _speedMultiplier = 1f;

    // 넉백 관련 변수 추가
    private float _knockbackTimer;

    private enum ChargeState { Approach, Dashing, Cooldown }
    private ChargeState _chargeState = ChargeState.Approach;
    private float _chargeStateTimer;
    private Vector2 _chargeDirection;

    // 진공청소기 장판 관련 변수
    private enum VacuumState { Ready, Preparing, Active, Cooldown }
    private VacuumState _vacuumState = VacuumState.Ready;
    private float _vacuumTimer;
    private bool _isPlayerSlowed = false;

    private PlayerController _playerController;
    private PlayerStats _playerStats;
    private static int _globalSlowCount = 0; // 다수의 청소기 슬로우 중첩 및 버그 방지용

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        _stats = GetComponent<EnemyStats>();
        _stats.OnDeath += () =>
        {
            _isDead = true;
            if (useVacuumArea && _isPlayerSlowed) ApplySlowToPlayer(false);
        };
    }

    private void Start()
    {
        // 씬에서 Player 태그로 자동 탐색
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            _target = player.transform;
            _playerController = player.GetComponent<PlayerController>();
            _playerStats = player.GetComponent<PlayerStats>();
        }
    }

    private void FixedUpdate()
    {
        if (_isDead || _target == null) return;

        // 넉백 중일 때는 물리 연산에 맡기고 모든 이동/기믹 AI를 잠시 중단합니다.
        if (_knockbackTimer > 0f)
        {
            _knockbackTimer -= Time.fixedDeltaTime;
            if (_knockbackTimer <= 0f)
            {
                _rb.velocity = Vector2.zero; // 넉백 종료 시 속도 초기화
            }
            return;
        }

        // 진공청소기 장판 기믹 실행
        if (useVacuumArea)
        {
            HandleVacuumArea();
        }

        // 준비 중이거나 장판 전개 중일 때는 이동 불가 처리
        bool canMove = true;
        if (useVacuumArea && (_vacuumState == VacuumState.Preparing || _vacuumState == VacuumState.Active))
        {
            canMove = false;
        }

        if (canMove)
        {
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
    }

    private void Update()
    {
        if (_isDead || _target == null) return;

        if (_attackTimer > 0f)
            _attackTimer -= Time.deltaTime;

        // 분무기 기믹: 쿨타임마다 투사체 발사 (넉백 중이 아닐 때만)
        if (useSprayShoot && projectilePrefab != null && _knockbackTimer <= 0f)
        {
            _shootTimer -= Time.deltaTime;
            if (_shootTimer <= 0f)
            {
                ShootProjectile();
                _shootTimer = shootCooldown;
            }
        }
    }

    // --- 넉백 적용 전용 public 함수 ---
    public void ApplyKnockback(Vector2 force, float duration)
    {
        _knockbackTimer = duration;
        _rb.velocity = Vector2.zero; // 기존에 받던 힘 초기화
        _rb.AddForce(force, ForceMode2D.Impulse); // 강력한 밀침 적용
    }

    // --- 진공청소기 장판 로직 ---
    private void HandleVacuumArea()
    {
        if (_vacuumState == VacuumState.Ready)
        {
            // 플레이어가 3 범위 내에 들어오면 즉시 멈추고 0.5초 대기 준비 상태로 돌입
            if (Vector2.Distance(transform.position, _target.position) <= vacuumTriggerRadius)
            {
                _vacuumState = VacuumState.Preparing;
                _vacuumTimer = vacuumPrepTime;
            }
        }
        else if (_vacuumState == VacuumState.Preparing)
        {
            _vacuumTimer -= Time.fixedDeltaTime;
            if (_vacuumTimer <= 0f)
            {
                // 0.5초가 지나면 2초간 슬로우 전개 상태로 돌입
                _vacuumState = VacuumState.Active;
                _vacuumTimer = vacuumActiveTime;
            }
        }
        else if (_vacuumState == VacuumState.Active)
        {
            _vacuumTimer -= Time.fixedDeltaTime;

            // 장판 전개 중에는 매 프레임 거리를 체크하여 범위 내에 있으면 슬로우 부여
            bool inRange = Vector2.Distance(transform.position, _target.position) <= vacuumTriggerRadius;
            ApplySlowToPlayer(inRange);

            if (_vacuumTimer <= 0f)
            {
                ApplySlowToPlayer(false); // 장판 종료 시 무조건 해제
                _vacuumState = VacuumState.Cooldown;
                _vacuumTimer = vacuumCooldown;
            }
        }
        else if (_vacuumState == VacuumState.Cooldown)
        {
            _vacuumTimer -= Time.fixedDeltaTime;
            if (_vacuumTimer <= 0f)
            {
                _vacuumState = VacuumState.Ready;
            }
        }
    }

    private void ApplySlowToPlayer(bool shouldSlow)
    {
        if (_playerController == null || _playerStats == null) return;

        if (shouldSlow && !_isPlayerSlowed)
        {
            _isPlayerSlowed = true;
            _globalSlowCount++;
            // 처음 슬로우 장판에 닿았을 때만 이동 속도를 절반으로 깎음
            if (_globalSlowCount == 1)
            {
                _playerController.MoveSpeed = _playerStats.moveSpeed * 0.5f;
            }
        }
        else if (!shouldSlow && _isPlayerSlowed)
        {
            _isPlayerSlowed = false;
            _globalSlowCount--;
            // 모든 청소기 장판에서 완전히 벗어났을 때만 이동 속도 원복
            if (_globalSlowCount <= 0)
            {
                _globalSlowCount = 0; // 안전장치
                _playerController.MoveSpeed = _playerStats.moveSpeed;
            }
        }
    }

    private void OnDestroy()
    {
        // 씬 전환이나 오브젝트 삭제 시 슬로우가 영구적으로 남는 버그 방지
        if (_isPlayerSlowed)
        {
            ApplySlowToPlayer(false);
        }
    }

    // --- 기타 로직 ---
    private void ShootProjectile()
    {
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Vector2 dir = (_target.position - transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.velocity = dir * 6f;
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }

    public void SetPattern(MovementPattern pattern)
    {
        _pattern = pattern;
        _chargeState = ChargeState.Approach;
    }

    private Vector2 GetAvoidanceVector()
    {
        if (!useAvoidance) return Vector2.zero;

        Vector2 avoidance = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, avoidanceRadius, enemyLayer);

        int count = 0;
        foreach (var neighbor in neighbors)
        {
            if (neighbor.gameObject != gameObject)
            {
                Vector2 diff = transform.position - neighbor.transform.position;
                avoidance += diff.normalized / (diff.magnitude + 0.1f);
                count++;
            }
        }

        if (count > 0) avoidance /= count;
        return avoidance * avoidanceForce;
    }

    private void ChaseTarget()
    {
        Vector2 direction = (_target.position - transform.position).normalized;
        Vector2 avoidance = GetAvoidanceVector();

        Vector2 finalDirection = (direction + avoidance).normalized;
        _rb.MovePosition(_rb.position + finalDirection * (_stats.Data.moveSpeed * _speedMultiplier * Time.fixedDeltaTime));
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
                ChaseTarget();
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
            moveDir = radialDir;
        }
        else if (distance < _orbitDistance - buffer)
        {
            moveDir = -radialDir;
        }
        else
        {
            Vector2 tangent = new Vector2(-radialDir.y, radialDir.x);
            moveDir = _orbitClockwise ? -tangent : tangent;
        }

        Vector2 avoidance = GetAvoidanceVector();
        Vector2 finalDirection = (moveDir + avoidance).normalized;

        _rb.MovePosition(_rb.position + finalDirection * (_stats.Data.moveSpeed * _orbitSpeedMultiplier * _speedMultiplier * Time.fixedDeltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (_pattern == MovementPattern.Charge && _chargeState == ChargeState.Dashing)
        {
            var playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(_stats.Data.damage);
                _attackTimer = _stats.Data.attackCooldown;
            }
        }
        else
        {
            TryAttack(other);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (_attackTimer > 0f) return;

        TryAttack(other);
    }

    private void TryAttack(Collider2D other)
    {
        var playerStats = other.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(_stats.Data.damage);
            _attackTimer = _stats.Data.attackCooldown;
        }
    }

    public void SetTarget(Transform newTarget) => _target = newTarget;
}