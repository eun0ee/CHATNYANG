using System.Collections;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    public string waveName = "Wave 1";
    public int enemyPrefabIndex = 0;
    public int spawnCount = 10;
    public float spawnInterval = 3f;   // 이 웨이브 내 반복 스폰 간격
    public float waveDuration = 60f;   // 이 웨이브 지속 시간(초)
}

public class WaveManager : MonoBehaviour
{
    [Header("Waves")]
    [SerializeField] private WaveData[] waves;

    [Header("References")]
    [SerializeField] private EnemySpawner spawner;

    private int _currentWaveIndex = 0;
    public float ElapsedTime { get; private set; }

    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    private void Update()
    {
        ElapsedTime += Time.deltaTime;
    }

    private IEnumerator RunWaves()
    {
        foreach (var wave in waves)
        {
            Debug.Log($"[Wave] {wave.waveName} 시작");
            yield return StartCoroutine(RunSingleWave(wave));
            _currentWaveIndex++;
        }

        // 모든 웨이브 끝나면 마지막 웨이브 무한 반복
        while (true)
        {
            var lastWave = waves[waves.Length - 1];
            yield return StartCoroutine(RunSingleWave(lastWave));
        }
    }

    private IEnumerator RunSingleWave(WaveData wave)
    {
        float timer = 0f;

        while (timer < wave.waveDuration)
        {
            spawner.Spawn(wave.enemyPrefabIndex, wave.spawnCount);

            float elapsed = 0f;
            while (elapsed < wave.spawnInterval)
            {
                elapsed += Time.deltaTime;
                timer += Time.deltaTime;
                yield return null;
            }
        }

        Debug.Log($"[WaveManager] {wave.waveName} 종료");
    }
}