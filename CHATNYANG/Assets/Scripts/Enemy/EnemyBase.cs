using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnRadius = 12f;      // 카메라 밖 거리
    [SerializeField] private float minSpawnRadius = 8f;    // 너무 가까이 스폰 방지

    private Transform _playerTransform;

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    /// <summary>
    /// WaveManager에서 호출 — count마리를 prefabIndex 종류로 스폰
    /// </summary>
    public void Spawn(int prefabIndex, int count)
    {
        if (prefabIndex >= enemyPrefabs.Length) return;
        StartCoroutine(SpawnRoutine(enemyPrefabs[prefabIndex], count));
    }

    private IEnumerator SpawnRoutine(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetSpawnPosition();
            Instantiate(prefab, pos, Quaternion.identity);
            yield return new WaitForSeconds(0.1f); // 한 번에 몰리지 않게
        }
    }

    private Vector2 GetSpawnPosition()
    {
        if (_playerTransform == null) return Random.insideUnitCircle * spawnRadius;

        Vector2 origin = _playerTransform.position;

        // 플레이어 주변 도넛 모양 영역에 랜덤 스폰
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Random.Range(minSpawnRadius, spawnRadius);

        return origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
}