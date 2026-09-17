using System.Collections.Generic;
using UnityEngine;

public class MissilePool : MonoBehaviour
{
    public static MissilePool Instance;

    [SerializeField] private BossMissile missilePrefab;
    [SerializeField] private int poolSize = 30;

    private Queue<BossMissile> missilePool = new Queue<BossMissile>();

    private void Awake()
    {
        Instance = this;

        CreatePool();
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            BossMissile missile = Instantiate(missilePrefab, transform);

            missile.gameObject.SetActive(false);

            missilePool.Enqueue(missile);
        }
    }

    public BossMissile GetMissile()
    {
        // 풀에 미사일이 없으면 하나 더 생성
        if (missilePool.Count == 0)
        {
            BossMissile newMissile = Instantiate(
                missilePrefab,
                transform
            );

            newMissile.gameObject.SetActive(false);

            missilePool.Enqueue(newMissile);
        }

        BossMissile missile = missilePool.Dequeue();

        missile.gameObject.SetActive(true);

        return missile;
    }

    public void ReturnMissile(BossMissile missile)
    {
        missile.gameObject.SetActive(false);

        missilePool.Enqueue(missile);
    }
}