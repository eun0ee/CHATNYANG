using UnityEngine;
using UnityEngine.Events;

public class ExperienceSystem : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int maxLevel = 100;
    [SerializeField] private AnimationCurve expCurve;

    public int   CurrentLevel { get; private set; } = 1;
    public float CurrentExp   { get; private set; } = 0f;
    public float RequiredExp  { get; private set; }

    public event UnityAction<int>          OnLevelUp;    // (새 레벨)
    public event UnityAction<float, float> OnExpChanged; // (현재, 필요)

    private void Start()
    {
        RequiredExp = CalcRequiredExp(CurrentLevel);
    }

    public void AddExperience(float amount)
    {
        if (CurrentLevel >= maxLevel) return;

        CurrentExp += amount;

        while (CurrentExp >= RequiredExp && CurrentLevel < maxLevel)
        {
            CurrentExp -= RequiredExp;
            CurrentLevel++;
            RequiredExp = CalcRequiredExp(CurrentLevel);
            OnLevelUp?.Invoke(CurrentLevel);
        }

        // 레벨업 처리 끝난 뒤 최종값으로 한 번만 발동
        OnExpChanged?.Invoke(CurrentExp, RequiredExp);
    }

    private float CalcRequiredExp(int level)
    {
        if (expCurve != null && expCurve.keys.Length > 0)
            return expCurve.Evaluate(level);

        return level * 100f + Mathf.Pow(level, 1.5f) * 10f;
    }
}