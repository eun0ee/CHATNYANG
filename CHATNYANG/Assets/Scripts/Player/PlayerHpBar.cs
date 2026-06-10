using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHpBar : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private PlayerStats playerStats; // 부모에서 자동 탐색 가능

    private void Awake()
    {
        // playerStats 인스펙터 미연결 시 부모에서 자동 탐색
        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();
    }

    private void Start()
    {
        if (playerStats == null) return;

        playerStats.OnHpChanged += RefreshHpUI;
        RefreshHpUI(playerStats.currentHp, playerStats.maxHp);
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnHpChanged -= RefreshHpUI;
    }

    private void RefreshHpUI(float current, float max)
    {
        if (hpSlider != null)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = max;
            hpSlider.value    = current;
        }
    }
}