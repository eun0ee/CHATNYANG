using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Smoothing")]
    [SerializeField] private bool useSmoothing = false;
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Bounds")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds; // 카메라가 갈 수 있는 최소 좌하단 좌표
    [SerializeField] private Vector2 maxBounds; // 카메라가 갈 수 있는 최대 우상단 좌표

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        // 제한 구역 사용 시 카메라 위치를 영역 내로 제한
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        transform.position = useSmoothing
            ? Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime)
            : desiredPosition;
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
}