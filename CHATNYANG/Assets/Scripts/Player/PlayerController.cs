using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private bool _isMoving;

    // 외부에서 속도 버프 적용할 수 있도록 프로퍼티로
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
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        GatherInput();
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
}