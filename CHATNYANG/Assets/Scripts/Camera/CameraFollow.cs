// CameraFollow.cs
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Smoothing")]
    [SerializeField] private bool useSmoothing = false;
    [SerializeField] private float smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        transform.position = useSmoothing
            ? Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime)
            : desiredPosition;
    }

    // 런타임에서 타겟 교체할 일 있을 때
    public void SetTarget(Transform newTarget) => target = newTarget;
}