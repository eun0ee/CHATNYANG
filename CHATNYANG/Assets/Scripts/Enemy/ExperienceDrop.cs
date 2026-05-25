using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class ExperienceDrop : MonoBehaviour
{
    private EnemyStats _stats;
    private static ExperienceSystem _expSystem; // static 캐싱 매번 Find 방지

    private void Awake()
    {
        _stats = GetComponent<EnemyStats>();
        _stats.OnDeath += DropExperience;

        if (_expSystem == null)
            _expSystem = FindObjectOfType<ExperienceSystem>();
    }

    private void DropExperience()
    {
        _expSystem?.AddExperience(_stats.Data.expReward);
    }

    private void OnDestroy()
    {
        _stats.OnDeath -= DropExperience;
    }
}