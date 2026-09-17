using UnityEngine;

public class BossMissile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;

    [SerializeField] private float lifeTime = 5f;

    private float timer;

    public void Initialize(Vector2 direction, float speed)
    {
        this.direction = direction.normalized;
        this.speed = speed;

        timer = 0f;

        // 미사일 방향 회전
        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);

        timer += Time.deltaTime;

        // 일정 시간이 지나면 풀로 반환
        if (timer >= lifeTime)
        {
            MissilePool.Instance.ReturnMissile(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 데미지 처리
            // collision.GetComponent<Player>()...

            MissilePool.Instance.ReturnMissile(this);
        }
    }
}