using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI 연결")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    private PlayerStats _playerStats;

    private void Start()
    {
        // Player 태그를 가진 오브젝트에서 PlayerStats를 찾음
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            _playerStats = player.GetComponent<PlayerStats>();

            if (_playerStats != null)
            {
                // 체력이 변할 때마다 UpdateUI 함수가 실행되도록 연결
                _playerStats.OnHpChanged += UpdateUI;

                // 시작할 때 한 번 UI 갱신
                UpdateUI(_playerStats.currentHp, _playerStats.maxHp);
            }
        }
    }

    private void UpdateUI(float currentHp, float maxHp)
    {
        if (hpSlider != null)
        {
            // value가 maxValue보다 크면 Unity가 자동으로 clamp해버려서
            // maxValue를 먼저 올린 뒤 value를 설정해야 함
            // 단, 이 순서 자체가 한 프레임 내 렌더링 틱과 겹치면 깜빡임 발생
            // → wholeNumbers + 명시적 순서로 고정
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }

        if (hpText != null)
            hpText.text = $"HP: {Mathf.CeilToInt(currentHp)}/{Mathf.CeilToInt(maxHp)}";
    }

    private void OnDestroy()
    {
        // 스크립트가 파괴될 때 이벤트 연결 해제 (메모리 누수 방지)
        if (_playerStats != null)
        {
            _playerStats.OnHpChanged -= UpdateUI;
        }
    }
}