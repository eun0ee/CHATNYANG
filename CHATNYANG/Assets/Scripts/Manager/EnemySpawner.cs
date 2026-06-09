using System.Collections;
using UnityEngine;

/// <summary>
/// 적을 플레이어 주변 특정 반경 안에 소환하는 컴포넌트.
/// WaveManager의 Spawn() 호출을 받아 코루틴으로 분산 소환합니다.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("소환 가능한 적 프리팹 목록. WaveData의 enemyPrefabIndex와 인덱스 일치 필요")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Position")]
    [Tooltip("플레이어로부터 소환될 최대 반경 (카메라 밖 기준으로 설정 권장)")]
    [SerializeField] private float spawnRadius = 12f;

    [Tooltip("플레이어로부터 소환될 최소 반경 (너무 가까운 소환 방지)")]
    [SerializeField] private float minSpawnRadius = 8f;

    [Header("Spawn Timing")]
    [Tooltip("한 번의 Spawn 호출에서 적 한 마리를 소환하는 간격 (초). 동시 소환으로 인한 스파이크 방지")]
    [SerializeField] private float spawnDelay = 0.1f;

    private Transform _playerTransform;

    private void Start()
    {
        // 태그가 "Player"인 오브젝트를 찾아 위치를 소환 기준으로 사용
        var player = GameObject.FindWithTag("Player");
        if (player != null)
            _playerTransform = player.transform;
        else
            Debug.LogWarning("[EnemySpawner] 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다.");
    }

    /// <summary>
    /// WaveManager에서 호출합니다.
    /// prefabIndex 번째 프리팹을 count만큼 분산 소환합니다.
    /// </summary>
    /// <param name="prefabIndex">enemyPrefabs 배열의 인덱스</param>
    /// <param name="count">소환할 적의 수</param>
    public void Spawn(int prefabIndex, int count)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("[EnemySpawner] enemyPrefabs 배열이 비어 있습니다.");
            return;
        }

        if (prefabIndex < 0 || prefabIndex >= enemyPrefabs.Length)
        {
            Debug.LogError($"[EnemySpawner] prefabIndex({prefabIndex})가 범위를 벗어났습니다. (배열 크기: {enemyPrefabs.Length})");
            return;
        }

        if (enemyPrefabs[prefabIndex] == null)
        {
            Debug.LogError($"[EnemySpawner] enemyPrefabs[{prefabIndex}]가 null입니다. Inspector에서 프리팹을 연결해주세요.");
            return;
        }

        StartCoroutine(SpawnRoutine(enemyPrefabs[prefabIndex], count));
    }

    /// <summary>
    /// 적을 spawnDelay 간격으로 하나씩 소환합니다.
    /// 한 프레임에 대량 소환 시 발생하는 성능 스파이크를 방지합니다.
    /// </summary>
    private IEnumerator SpawnRoutine(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetSpawnPosition();
            Instantiate(prefab, pos, Quaternion.identity);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    /// <summary>
    /// 플레이어 기준 minSpawnRadius ~ spawnRadius 사이의 랜덤한 위치를 반환합니다.
    /// 플레이어가 없으면 월드 원점 기준으로 대체합니다.
    /// </summary>
    private Vector2 GetSpawnPosition()
    {
        Vector2 origin = _playerTransform != null
            ? (Vector2)_playerTransform.position
            : Vector2.zero;

        // 0 ~ 2π 사이의 랜덤 각도
        float angle  = Random.Range(0f, Mathf.PI * 2f);

        // 최소~최대 반경 사이의 랜덤 거리
        float radius = Random.Range(minSpawnRadius, spawnRadius);

        return origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
}