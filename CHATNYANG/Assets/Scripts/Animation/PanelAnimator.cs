using UnityEngine;
using DG.Tweening;

public class PanelAnimator : MonoBehaviour
{
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private Ease easeType = Ease.OutBack;

    private RectTransform _rect;
    private CanvasGroup _canvasGroup;

    public bool IsVisible => _canvasGroup != null && _canvasGroup.blocksRaycasts;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // Awake 대신 Start에서 숨김 초기화
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

        _rect.DOScale(Vector3.one, duration)
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