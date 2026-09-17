using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField] private int missileSpeed = 5;

    // Update is called once per frame
    void Start()
    {
        StartCoroutine(MissileStart());
    }

    IEnumerator MissileStart()
    {
        while (true)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = Mathf.PI * 2f * i / 8f;

                Vector2 direction = new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle)
                ).normalized;

                BossMissile missile = MissilePool.Instance.GetMissile();

                missile.transform.position = transform.position;

                missile.Initialize(
                    direction,
                    missileSpeed
                );
            }

            yield return new WaitForSeconds(2f);
        }
    }
}
