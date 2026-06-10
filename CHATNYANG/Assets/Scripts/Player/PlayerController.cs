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
        // �̵� ���� ���� ���� �Ķ���� ������Ʈ
        if (_isMoving)
        {
            _animator.SetFloat("MoveX", _moveInput.x);
            _animator.SetFloat("MoveY", _moveInput.y);
        }

        // �̵� �ӵ��� �����Ͽ� ���� �ȱ� ���� ��ȯ
        _animator.SetFloat("Speed", _moveInput.sqrMagnitude);
    }

    private void FlipSprite()
    {
        // �⺻ �ִϸ��̼��� ���� �����̹Ƿ�, ���������� �̵��� �� �̹����� ������
        if (_moveInput.x > 0)
        {
            _spriteRenderer.flipX = true;
        }
        // �������� �̵��� ���� ���� �̹��� �״�� ���
        else if (_moveInput.x < 0)
        {
            _spriteRenderer.flipX = false;
        }
    }
}