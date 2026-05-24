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
        _animator.SetBool(IsMoving, _controller.IsMoving);

        if (_controller.IsMoving)
        {
            _animator.SetFloat(MoveX, _controller.MoveDirection.x);
            _animator.SetFloat(MoveY, _controller.MoveDirection.y);

            // 좌우 반전으로 스프라이트 절약
            if (_controller.MoveDirection.x != 0)
                _spriteRenderer.flipX = _controller.MoveDirection.x < 0;
        }
    }
}