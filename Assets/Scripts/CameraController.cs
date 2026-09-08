using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target; // 따라갈 대상 (플레이어)
    private Vector3 offset = new Vector3(0f, 1f, -10f);

    void Start()
    {
        if (target == null)
        {
            FindPlayer();
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            FindPlayer();
            if (target == null) return;
        }

        // 보간 없이 즉시 위치 고정 (떨림 원천 차단)
        transform.position = target.position + offset;
    }

    private void FindPlayer()
    {
        // 최신 유니티 추천 방식 (FindAnyObjectByType이 검색 속도가 더 빠름)
        PlayerController playerObj = FindAnyObjectByType<PlayerController>();
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }
}