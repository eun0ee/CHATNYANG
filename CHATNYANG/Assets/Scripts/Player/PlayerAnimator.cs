using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");

    private PlayerController _controller;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_animator == null) return;

        // 파라미터 존재 여부 체크 후 설정
        if (HasParameter("IsMoving"))
            _animator.SetBool(IsMoving, _controller.IsMoving);

        if (_controller.IsMoving)
        {
            if (HasParameter("MoveX"))
                _animator.SetFloat(MoveX, _controller.MoveDirection.x);
            if (HasParameter("MoveY"))
                _animator.SetFloat(MoveY, _controller.MoveDirection.y);

            if (_controller.MoveDirection.x != 0)
                _spriteRenderer.flipX = _controller.MoveDirection.x < 0;
        }
    }

    private bool HasParameter(string paramName)
    {
        if (_animator == null) return false;
        foreach (var param in _animator.parameters)
            if (param.name == paramName) return true;
        return false;
    }
}