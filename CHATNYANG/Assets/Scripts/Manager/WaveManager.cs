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
/// 웨이브 진행을 총괄하는 매니저.
/// waves 배열 순서대로 웨이브를 실행하며,
/// 마지막 웨이브가 끝나면 해당 웨이브를 무한 반복합니다.
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Waves")]
    [SerializeField] private WaveData[] waves;

    [Header("References")]
    [SerializeField] private EnemySpawner spawner;

    /// <summary>게임 시작 이후 누적 경과 시간 (초)</summary>
    public float ElapsedTime { get; private set; }

    // 게임 종료 시 무한 루프를 중단하기 위한 플래그
    private bool _isGameOver = false;

    private void Start()
    {
        if (waves == null || waves.Length == 0)
        {
            Debug.LogWarning("[WaveManager] waves 배열이 비어 있습니다.");
            return;
        }

        if (spawner == null)
        {
            Debug.LogError("[WaveManager] EnemySpawner가 연결되지 않았습니다.");
            return;
        }

        StartCoroutine(RunWaves());
    }

    private void Update()
    {
        ElapsedTime += Time.deltaTime;
    }

    /// <summary>
    /// 모든 웨이브를 순서대로 실행합니다.
    /// 마지막 웨이브 이후에는 해당 웨이브를 무한 반복합니다.
    /// </summary>
    private IEnumerator RunWaves()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            Debug.Log($"[WaveManager] {waves[i].waveName} 시작 (Wave {i + 1}/{waves.Length})");
            yield return StartCoroutine(RunSingleWave(waves[i]));
            Debug.Log($"[WaveManager] {waves[i].waveName} 완료");
        }

        // 모든 웨이브 클리어 후 마지막 웨이브 무한 반복
        WaveData lastWave = waves[waves.Length - 1];
        Debug.Log("[WaveManager] 마지막 웨이브 무한 반복 시작");

        while (!_isGameOver)
        {
            yield return StartCoroutine(RunSingleWave(lastWave));
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
                timer  += dt;

                // waveDuration 초과 시 즉시 웨이브 종료 (초과 스폰 방지)
                if (timer >= wave.waveDuration) yield break;

                yield return null;
            }
        }
    }

    /// <summary>
    /// 게임 오버 또는 씬 전환 시 호출해 무한 루프를 중단합니다.
    /// 예) GameManager에서 OnGameOver 이벤트 연결
    /// </summary>
    public void StopWaves()
    {
        _isGameOver = true;
        StopAllCoroutines();
        Debug.Log("[WaveManager] 웨이브 중단됨");
    }
}