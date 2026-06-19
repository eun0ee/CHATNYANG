using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    private float remainingTime = 600f; // 10분
    private bool isTimerRunning = true;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI coinCountText;
    private int killCount = 0;
    private int coinCount = 0;

    [Header("Buttons")]
    [SerializeField] private Button stopButton;
    [SerializeField] private Button settingButton;
    private bool isStopped = false;

    [Header("Panels")]
    [SerializeField] private GameObject settingPanel;

    [Header("Experience")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private ExperienceSystem expSystem;
    
    [Header("Panels")]
    [SerializeField] private PanelAnimator settingPanelAnimator;

    [Header("Game Over")]
    [SerializeField] private PanelAnimator gameOverPanelAnimator;
    [SerializeField] private Button titleButton;

    [Header("Game Over Results")]
    [SerializeField] private TextMeshProUGUI resultSurvivalTimeText;
    [SerializeField] private TextMeshProUGUI resultLevelText;
    [SerializeField] private TextMeshProUGUI resultKillCountText;

    [Header("Game Over Weapon History")]
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private WeaponSlotUI[] resultWeaponSlots; // 게임오버 창에 배치한 슬롯들
    [SerializeField] private Sprite normalAura;
    [SerializeField] private Sprite rareAura;
    [SerializeField] private Sprite epicAura;
    [SerializeField] private Sprite uniqueAura;
    [SerializeField] private Sprite legendaryAura;

    // 현재 레벨을 추적하기 위한 변수 추가
    private int currentLevel = 1;


    // ───────────────────────────────────────────────
    #region Unity Lifecycle

    private void Awake()
    {
        // 싱글턴
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        stopButton.onClick.AddListener(OnStopButtonClicked);
        settingButton.onClick.AddListener(OnSettingButtonClicked);
        titleButton.onClick.AddListener(OnTitleButtonClicked);

        if (expSystem != null)
        {
            expSystem.OnExpChanged += RefreshExpUI;
            expSystem.OnLevelUp    += RefreshLevelUI;
        }

        UpdateTimerUI();
        UpdateKillUI();
        UpdateCoinUI();
        RefreshExpUI(0f, expSystem != null ? expSystem.RequiredExp : 100f);
        RefreshLevelUI(1);
    }

    private void OnDestroy()
    {
        if (expSystem != null)
        {
            expSystem.OnExpChanged -= RefreshExpUI;
            expSystem.OnLevelUp    -= RefreshLevelUI;
        }
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isTimerRunning = false;
            OnTimerEnd();
        }

        UpdateTimerUI();
    }

    #endregion

    // ───────────────────────────────────────────────
    #region Exp / Level UI

    // OnExpChanged(currentExp, requiredExp) 수신
    private void RefreshExpUI(float current, float required)
    {
        if (expSlider != null)
        {
            expSlider.minValue = 0f;
            expSlider.maxValue = required;
            expSlider.value    = current;
        }
    }

    // OnLevelUp(newLevel) 수신
    private void RefreshLevelUI(int newLevel)
    {
        currentLevel = newLevel; // 도달 레벨 저장

        if (levelText != null)
            levelText.text = $"Lv. {newLevel}";

        if (expSystem != null)
            RefreshExpUI(expSystem.CurrentExp, expSystem.RequiredExp);
    }

    #endregion

    // ───────────────────────────────────────────────
    #region Timer

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnTimerEnd()
    {
        Debug.Log("타이머 종료!");
        // TODO: 게임 종료 / 결과 화면 처리
    }

    #endregion

    // ───────────────────────────────────────────────
    #region Public API — Stats

    public void AddKill(int amount = 1)
    {
        killCount += amount;
        UpdateKillUI();
    }

    public void AddCoin(int amount = 1)
    {
        coinCount += amount;
        UpdateCoinUI();
    }

    private void UpdateKillUI() => killCountText.text = $"{killCount}";
    private void UpdateCoinUI() => coinCountText.text  = $"{coinCount}";

    #endregion

    // ───────────────────────────────────────────────
    #region Button Handlers

    private void OnStopButtonClicked()
    {
        isStopped = !isStopped;
        isTimerRunning = !isStopped;
        Time.timeScale = isStopped ? 0f : 1f;

        // 버튼 텍스트 토글 (선택)
        var label = stopButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = isStopped ? "▶ 재개" : "⏸ 정지";
    }

    private void OnSettingButtonClicked()
    {
        // PanelAnimator의 blocksRaycasts로 열림 여부 판단
        bool open = !settingPanelAnimator.IsVisible;

        if (open)
        {
            isTimerRunning = false;
            Time.timeScale = 0f;
            settingPanelAnimator.Show();
        }
        else
        {
            CloseSettingPanel();
        }
    }

    public void CloseSettingPanel()
    {
        settingPanelAnimator.Hide(() =>
        {
            isTimerRunning = true;
            Time.timeScale = 1f;
        });
    }

    #endregion

    // PlayerStats.Die()에서 호출
    public void ShowGameOver()
    {
        isTimerRunning = false;
        Time.timeScale = 0f;

        // 1. 결과 텍스트 업데이트
        UpdateGameOverTexts();

        // 2. 무기 히스토리 업데이트
        UpdateGameOverWeapons();

        gameOverPanelAnimator.Show();
    }

    private void UpdateGameOverTexts()
    {
        // 생존 시간 계산 (전체 시간 600초에서 남은 시간을 뺌)
        float survivedTime = 600f - remainingTime;
        int minutes = Mathf.FloorToInt(survivedTime / 60f);
        int seconds = Mathf.FloorToInt(survivedTime % 60f);

        if (resultSurvivalTimeText != null)
            resultSurvivalTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (resultLevelText != null)
            resultLevelText.text = $"Lv. {currentLevel}";

        if (resultKillCountText != null)
            resultKillCountText.text = $"{killCount}";
    }

    private void UpdateGameOverWeapons()
    {
        if (weaponManager == null || resultWeaponSlots == null) return;

        // 읽기 전용으로 현재 무기 리스트 가져오기
        var currentWeapons = weaponManager.Weapons;

        for (int i = 0; i < resultWeaponSlots.Length; i++)
        {
            if (i < currentWeapons.Count)
            {
                WeaponBase weapon = currentWeapons[i];
                Sprite aura = GetAuraByRarity(weapon.currentRarity);

                // WeaponSlotUI의 UpdateSlot 함수를 호출하여 아우라, 아이콘, 강화수치 적용
                resultWeaponSlots[i].UpdateSlot(weapon.WeaponData.weaponIcon, aura, weapon.currentUpgradeLevel);
            }
            else
            {
                // 무기가 없는 빈 슬롯 처리
                resultWeaponSlots[i].ClearSlot();
            }
        }
    }

    private Sprite GetAuraByRarity(WeaponRarity rarity)
    {
        switch (rarity)
        {
            case WeaponRarity.Normal: return normalAura;
            case WeaponRarity.Rare: return rareAura;
            case WeaponRarity.Epic: return epicAura;
            case WeaponRarity.Unique: return uniqueAura;
            case WeaponRarity.Legendary: return legendaryAura;
            default: return normalAura;
        }
    }

    private void OnTitleButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }
}