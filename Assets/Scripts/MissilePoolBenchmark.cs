using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class MissilePoolBenchmarkSimple : MonoBehaviour
{
    [SerializeField] private BossMissile missilePrefab;
    [SerializeField] private int missilesPerBatch = 8;
    [SerializeField] private int warmupBatches = 10;
    [SerializeField] private int measuredBatches = 60;

    private bool isRunning;

    [ContextMenu("Run API Time Benchmark (Play Mode)")]
    private void RunBenchmark()
    {
        if (!Application.isPlaying || isRunning)
            return;

        if (missilePrefab == null || MissilePool.Instance == null)
        {
            UnityEngine.Debug.LogError(
                "MissilePrefab 또는 MissilePool을 확인하세요."
            );
            return;
        }

        StartCoroutine(BenchmarkRoutine());
    }

    private IEnumerator BenchmarkRoutine()
    {
        isRunning = true;

        var batch = new BossMissile[missilesPerBatch];
        var timer = new Stopwatch();

        double createDestroyTotal = 0;
        double poolTotal = 0;

        // 예열
        for (int i = 0; i < warmupBatches; i++)
        {
            TestInstantiateDestroy(batch, timer);
            yield return null;

            TestPool(batch, timer);
            yield return null;
        }

        // 측정 순서를 번갈아 실행
        for (int i = 0; i < measuredBatches; i++)
        {
            double a, b;

            if (i % 2 == 0)
            {
                a = TestInstantiateDestroy(batch, timer);
                yield return null;

                b = TestPool(batch, timer);
                yield return null;
            }
            else
            {
                b = TestPool(batch, timer);
                yield return null;

                a = TestInstantiateDestroy(batch, timer);
                yield return null;
            }

            createDestroyTotal += a;
            poolTotal += b;
        }

        double createAvg = createDestroyTotal / measuredBatches;
        double poolAvg = poolTotal / measuredBatches;

        double reduction = createAvg > 0
            ? (createAvg - poolAvg) / createAvg * 100
            : 0;

        UnityEngine.Debug.Log(
            $"[Missile Benchmark]\n" +
            $"Instantiate + Destroy: {createAvg:F4} ms/batch\n" +
            $"Pool Get + Return: {poolAvg:F4} ms/batch\n" +
            $"미사일당 생성/파괴: " +
            $"{createAvg * 1000 / missilesPerBatch:F2} μs\n" +
            $"미사일당 풀 사용: " +
            $"{poolAvg * 1000 / missilesPerBatch:F2} μs\n" +
            $"API 호출 시간 감소율: {reduction:F1}%"
        );

        isRunning = false;
    }

    private double TestInstantiateDestroy(
        BossMissile[] batch, Stopwatch timer)
    {
        timer.Restart();

        for (int i = 0; i < batch.Length; i++)
            batch[i] = Instantiate(missilePrefab, transform);

        timer.Stop();
        double createTime = timer.Elapsed.TotalMilliseconds;

        // 화면 밖으로 이동하는 비용은 측정에서 제외
        for (int i = 0; i < batch.Length; i++)
            batch[i].transform.position =
                transform.position + Vector3.right * (10000 + i * 5);

        timer.Restart();

        for (int i = 0; i < batch.Length; i++)
            Destroy(batch[i].gameObject);

        timer.Stop();

        return createTime + timer.Elapsed.TotalMilliseconds;
    }

    private double TestPool(BossMissile[] batch, Stopwatch timer)
    {
        var pool = MissilePool.Instance;

        timer.Restart();

        for (int i = 0; i < batch.Length; i++)
            batch[i] = pool.GetMissile();

        timer.Stop();
        double getTime = timer.Elapsed.TotalMilliseconds;

        // 화면 밖으로 이동하는 비용은 측정에서 제외
        for (int i = 0; i < batch.Length; i++)
            batch[i].transform.position =
                transform.position + Vector3.right * (10000 + i * 5);

        timer.Restart();

        for (int i = 0; i < batch.Length; i++)
            pool.ReturnMissile(batch[i]);

        timer.Stop();

        return getTime + timer.Elapsed.TotalMilliseconds;
    }
}