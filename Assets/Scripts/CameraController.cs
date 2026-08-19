using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target; // 따라갈 대상 (플레이어)
    private Vector3 offset = new Vector3(0f, 1f, -10f);

    void LateUpdate()
    {
        if (target == null) return;

        // 보간 없이 즉시 위치 고정 (떨림 원천 차단)
        transform.position = target.position + offset;
    }
}