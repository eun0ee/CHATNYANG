using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class ExperienceDrop : MonoBehaviour
{
    [SerializeField] private GameObject expOrbPrefab; // ExpOrb 프리팹 연결

    private EnemyStats _stats;
    private static ExperienceSystem _expSystem;

    private void Awake()
    {
        _stats = GetComponent<EnemyStats>();
        _stats.OnDeath += DropExperience;

        if (_expSystem == null)
            _expSystem = FindObjectOfType<ExperienceSystem>();
    }

    private void DropExperience()
    {
        if (expOrbPrefab == null || _expSystem == null) return;

        GameObject orb = Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
        orb.GetComponent<ExpOrb>()?.Init(_stats.Data.expReward, _expSystem);
    }

    private void OnDestroy()
    {
        if (_stats != null)
            _stats.OnDeath -= DropExperience;
    }
}