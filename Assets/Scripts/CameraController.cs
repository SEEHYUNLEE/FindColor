using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
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

        transform.position = target.position + offset;
    }

    private void FindPlayer()
    {
        PlayerController playerObj = FindAnyObjectByType<PlayerController>();
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }
}