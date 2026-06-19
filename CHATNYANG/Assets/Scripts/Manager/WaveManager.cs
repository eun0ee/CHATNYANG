using System.Collections;
using UnityEngine;

/// <summary>
/// 웨이브 하나의 설정 데이터.
/// Inspector에서 배열로 추가해 웨이브 구성을 직접 편집할 수 있습니다.
/// </summary>
[System.Serializable]
public class WaveData
{
    [Tooltip("Inspector에서 구분하기 위한 웨이브 이름")]
    public string waveName = "Wave 1";

    [Tooltip("EnemySpawner의 enemyPrefabs 배열 인덱스")]
    public int enemyPrefabIndex = 0;

    [Tooltip("스폰 1회당 소환할 적의 수")]
    public int spawnCount = 10;

    [Tooltip("스폰과 스폰 사이의 간격 (초)")]
    public float spawnInterval = 3f;

    [Tooltip("이 웨이브가 지속되는 총 시간 (초)")]
    public float waveDuration = 60f;
}

/// <summary>
/// 일반 웨이브를 모두 진행한 후,
/// 무한 반복용 웨이브 배열을 계속해서 순환하는 매니저.
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Normal Waves (Plays Once)")]
    [SerializeField] private WaveData[] normalWaves;

    [Header("Infinite Loop Waves (Repeats Forever)")]
    [SerializeField] private WaveData[] infiniteLoopWaves;

    [Header("Background Spawner")]
    [SerializeField] private bool enableBackgroundSpawn = true;
    [SerializeField] private int backgroundEnemyIndex = 0; // 초기 생성될 기본 적 인덱스 (Enemy 0)
    [SerializeField] private int backgroundSpawnCount = 3; // 백그라운드에서 한 번에 생성될 수
    [SerializeField] private float backgroundSpawnInterval = 5f; // 백그라운드 생성 주기
    [SerializeField] private int[] backgroundUpgradeIndices; // 이 배열에 있는 인덱스의 적이 웨이브에 등장하면, 백그라운드 적도 이것으로 교체됨

    [Header("References")]
    [SerializeField] private EnemySpawner spawner;

    /// <summary>게임 시작 이후 누적 경과 시간 (초)</summary>
    public float ElapsedTime { get; private set; }

    // 게임 종료 시 무한 루프를 중단하기 위한 플래그
    private bool _isGameOver = false;

    private void Start()
    {
        if (spawner == null)
        {
            Debug.LogError("[WaveManager] EnemySpawner is missing");
            return;
        }

        StartCoroutine(RunWaves());

        // 백그라운드 무한 스폰 코루틴 시작
        if (enableBackgroundSpawn)
        {
            StartCoroutine(BackgroundSpawnRoutine());
        }
    }

    private void Update()
    {
        ElapsedTime += Time.deltaTime;
    }

    // 메인 웨이브와 별개로 독립적으로 계속해서 적을 스폰하는 로직
    private IEnumerator BackgroundSpawnRoutine()
    {
        while (!_isGameOver)
        {
            yield return new WaitForSeconds(backgroundSpawnInterval);

            if (!_isGameOver)
            {
                spawner.Spawn(backgroundEnemyIndex, backgroundSpawnCount);
            }
        }
    }

    /// <summary>
    /// 일반 웨이브를 1회씩 실행한 뒤, 
    /// 설정된 무한 루프 웨이브들을 순환합니다.
    /// </summary>
    private IEnumerator RunWaves()
    {
        // 1. 일반 웨이브 1회씩 순차 진행
        if (normalWaves != null)
        {
            for (int i = 0; i < normalWaves.Length; i++)
            {
                if (_isGameOver) yield break;

                // 웨이브가 시작될 때 백그라운드 스폰 적 교체 여부 검사
                CheckAndUpgradeBackgroundSpawner(normalWaves[i].enemyPrefabIndex);

                Debug.Log($"[WaveManager] Normal Wave Started: {normalWaves[i].waveName}");
                yield return StartCoroutine(RunSingleWave(normalWaves[i]));
            }
        }

        // 2. 무한 반복 웨이브들을 게임 오버 전까지 무한 순환
        if (infiniteLoopWaves != null && infiniteLoopWaves.Length > 0)
        {
            Debug.Log("[WaveManager] Starting Infinite Loop Waves");
            while (!_isGameOver)
            {
                for (int i = 0; i < infiniteLoopWaves.Length; i++)
                {
                    if (_isGameOver) yield break;

                    CheckAndUpgradeBackgroundSpawner(infiniteLoopWaves[i].enemyPrefabIndex);

                    Debug.Log($"[WaveManager] Infinite Wave Started: {infiniteLoopWaves[i].waveName}");
                    yield return StartCoroutine(RunSingleWave(infiniteLoopWaves[i]));
                }
            }
        }
    }

    // 현재 웨이브의 적 인덱스가 교체 리스트에 있다면 백그라운드 스폰 적을 업데이트
    private void CheckAndUpgradeBackgroundSpawner(int currentWaveEnemyIndex)
    {
        if (!enableBackgroundSpawn || backgroundUpgradeIndices == null) return;

        for (int i = 0; i < backgroundUpgradeIndices.Length; i++)
        {
            if (currentWaveEnemyIndex == backgroundUpgradeIndices[i])
            {
                if (backgroundEnemyIndex != currentWaveEnemyIndex)
                {
                    backgroundEnemyIndex = currentWaveEnemyIndex;
                    Debug.Log($"[WaveManager] Background spawner upgraded to index: {backgroundEnemyIndex}");
                }
                break;
            }
        }
    }

    /// <summary>
    /// 웨이브 1개를 실행합니다.
    /// waveDuration 동안 spawnInterval마다 Spawn을 호출합니다.
    /// </summary>
    private IEnumerator RunSingleWave(WaveData wave)
    {
        float timer = 0f;

        while (timer < wave.waveDuration)
        {
            // 스폰 호출
            spawner.Spawn(wave.enemyPrefabIndex, wave.spawnCount);

            // spawnInterval만큼 대기하면서 타이머도 함께 누적
            float elapsed = 0f;
            while (elapsed < wave.spawnInterval)
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                timer += dt;

                // waveDuration 초과 시 즉시 웨이브 종료 (초과 스폰 방지)
                if (timer >= wave.waveDuration) yield break;

                yield return null;
            }
        }
    }

    /// <summary>
    /// 게임 오버 또는 씬 전환 시 호출해 무한 루프를 중단합니다.
    /// </summary>
    public void StopWaves()
    {
        _isGameOver = true;
        StopAllCoroutines();
        Debug.Log("[WaveManager] Waves Stopped");
    }
}