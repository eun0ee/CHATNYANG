using UnityEngine;

public class HPSliderFollower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private float smoothSpeed = 15f; // 높을수록 즉각 반응

    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + worldOffset);

        bool isVisible = screenPos.z > 0f;
        gameObject.SetActive(isVisible);
        if (!isVisible) return;

        // 픽셀 반올림으로 진동 방지
        screenPos.x = Mathf.Round(screenPos.x);
        screenPos.y = Mathf.Round(screenPos.y);

        // 스무딩으로 부드럽게 추적
        _rect.position = Vector3.Lerp(_rect.position, screenPos, Time.deltaTime * smoothSpeed);
    }
}