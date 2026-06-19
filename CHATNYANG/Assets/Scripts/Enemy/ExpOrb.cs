using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ExpOrb : MonoBehaviour
{
    [Header("Orb Settings")]
    [SerializeField] private float attractRadius = 3f;
    [SerializeField] private float moveSpeed = 6f;

    private float _expAmount;
    private ExperienceSystem _expSystem;
    private Transform _target;
    private bool _isAttracting = false;

    // ExperienceDrop에서 호출
    public void Init(float amount, ExperienceSystem expSystem)
    {
        _expAmount = amount;
        _expSystem = expSystem;
    }

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) _target = player.transform;

        // 흡수 감지용 트리거 콜라이더 크기 설정
        GetComponent<CircleCollider2D>().radius = attractRadius;

        // 물리 충돌 방지용 트리거 및 키네마틱 강제 설정
        GetComponent<CircleCollider2D>().isTrigger = true;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
        }
    }

    private void Update()
    {
        if (!_isAttracting || _target == null) return;

        // 플레이어 방향으로 이동
        transform.position = Vector2.MoveTowards(
            transform.position,
            _target.position,
            moveSpeed * Time.deltaTime
        );
    }

    // 흡수 범위 진입 → 추적 시작
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _isAttracting = true;
    }

    // 추적 중 완전히 닿았을 때 획득 (Trigger끼리 겹칠 경우 대비)
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        float dist = Vector2.Distance(transform.position, other.transform.position);
        if (dist < 0.2f) Pickup();
    }

    private void Pickup()
    {
        _expSystem?.AddExperience(_expAmount);
        Destroy(gameObject);
    }
}