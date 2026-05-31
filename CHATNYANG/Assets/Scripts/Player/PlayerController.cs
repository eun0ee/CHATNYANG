using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _moveInput;
    private bool _isMoving;

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    public Vector2 MoveDirection => _moveInput;
    public bool IsMoving => _isMoving;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        GatherInput();
        UpdateAnimation();
        FlipSprite();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void GatherInput()
    {
        _moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        _isMoving = _moveInput != Vector2.zero;
    }

    private void Move()
    {
        _rb.MovePosition(_rb.position + _moveInput * (moveSpeed * Time.fixedDeltaTime));
    }

    private void UpdateAnimation()
    {
        // 이동 중일 때만 방향 파라미터 업데이트
        if (_isMoving)
        {
            _animator.SetFloat("MoveX", _moveInput.x);
            _animator.SetFloat("MoveY", _moveInput.y);
        }

        // 이동 속도를 전달하여 대기와 걷기 상태 전환
        _animator.SetFloat("Speed", _moveInput.sqrMagnitude);
    }

    private void FlipSprite()
    {
        // 기본 애니메이션이 왼쪽 방향이므로, 오른쪽으로 이동할 때 이미지를 뒤집음
        if (_moveInput.x > 0)
        {
            _spriteRenderer.flipX = true;
        }
        // 왼쪽으로 이동할 때는 원본 이미지 그대로 사용
        else if (_moveInput.x < 0)
        {
            _spriteRenderer.flipX = false;
        }
    }
}