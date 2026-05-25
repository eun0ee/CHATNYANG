using UnityEngine;
using UnityEngine.Events;

public class ExperienceSystem : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int maxLevel = 40;
    [SerializeField] private AnimationCurve expCurve; // Inspector에서 커브로 경험치 테이블 조정

    public int CurrentLevel { get; private set; } = 1;
    public float CurrentExp { get; private set; } = 0f;
    public float RequiredExp { get; private set; }

    // 레벨업 시 LevelUpUI가 구독
    public event UnityAction OnLevelUp;
    public event UnityAction<float, float> OnExpChanged; // (current, required)

    private void Start()
    {
        RequiredExp = GetRequiredExp(CurrentLevel);
    }

    public void AddExperience(float amount)
    {
        if (CurrentLevel >= maxLevel) return;

        CurrentExp += amount;
        Debug.Log($"[ExpSystem] 경험치 획득: +{amount} / 현재: {CurrentExp}/{RequiredExp}");
        OnExpChanged?.Invoke(CurrentExp, RequiredExp);

        while (CurrentExp >= RequiredExp && CurrentLevel < maxLevel)
        {
            CurrentExp -= RequiredExp;
            CurrentLevel++;
            RequiredExp = GetRequiredExp(CurrentLevel);
            Debug.Log($"[ExpSystem] 레벨업! 현재 레벨: {CurrentLevel}");
            OnLevelUp?.Invoke();
        }
    }

    // AnimationCurve 없이도 쓸 수 있는 기본 공식
    // Inspector에서 expCurve 설정하면 커브 우선 사용
    private float GetRequiredExp(int level)
    {
        if (expCurve != null && expCurve.keys.Length > 0)
            return expCurve.Evaluate(level);

        // 기본 공식: 레벨 * 100 + (레벨^1.5 * 10)
        return level * 100f + Mathf.Pow(level, 1.5f) * 10f;
    }
}