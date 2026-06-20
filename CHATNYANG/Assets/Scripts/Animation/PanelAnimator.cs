using UnityEngine;
using DG.Tweening;

public class PanelAnimator : MonoBehaviour
{
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private Ease easeType = Ease.OutBack;

    private RectTransform _rect;
    private CanvasGroup _canvasGroup;

    // 원래 설정된 스케일 값을 기억하기 위한 변수
    private Vector3 _originalScale;

    public bool IsVisible => _canvasGroup != null && _canvasGroup.blocksRaycasts;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();

        // 게임 시작 시 Inspector에 설정된 원래 크기를 저장합니다.
        _originalScale = _rect.localScale;

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        HideImmediate();
    }

    private void HideImmediate()
    {
        _rect.DOKill(true);
        _canvasGroup.DOKill(true);

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _rect.localScale = Vector3.zero;
    }

    public void Show()
    {
        HideImmediate();

        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        // Vector3.one 대신 저장해둔 _originalScale을 사용해 원래 크기로 복구합니다.
        _rect.DOScale(_originalScale, duration)
             .SetEase(easeType)
             .SetUpdate(true);

        _canvasGroup.DOFade(1f, duration * 0.6f)
                    .SetUpdate(true);
    }

    public void Hide(System.Action onComplete = null)
    {
        _rect.DOKill(true);
        _canvasGroup.DOKill(true);

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _rect.DOScale(Vector3.zero, duration * 0.7f)
             .SetEase(Ease.InBack)
             .SetUpdate(true)
             .OnComplete(() => onComplete?.Invoke());

        _canvasGroup.DOFade(0f, duration * 0.5f)
                    .SetUpdate(true);
    }
}