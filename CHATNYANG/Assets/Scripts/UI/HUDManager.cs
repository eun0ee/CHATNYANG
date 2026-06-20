using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    // 카운트다운 대신 0부터 시작하는 경과 시간 변수로 변경
    private float elapsedTime = 0f;
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
    [SerializeField] private WeaponSlotUI[] resultWeaponSlots;
    [SerializeField] private Sprite normalAura;
    [SerializeField] private Sprite rareAura;
    [SerializeField] private Sprite epicAura;
    [SerializeField] private Sprite uniqueAura;
    [SerializeField] private Sprite legendaryAura;

    private int currentLevel = 1;

    private Vector3 _survivalOriginalScale;
    private Vector3 _levelOriginalScale;
    private Vector3 _killOriginalScale;
    private Vector3[] _weaponOriginalScales;

    // ───────────────────────────────────────────────
    #region Unity Lifecycle

    private void Awake()
    {
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
            expSystem.OnLevelUp += RefreshLevelUI;
        }

        if (resultSurvivalTimeText != null) _survivalOriginalScale = resultSurvivalTimeText.rectTransform.localScale;
        if (resultLevelText != null) _levelOriginalScale = resultLevelText.rectTransform.localScale;
        if (resultKillCountText != null) _killOriginalScale = resultKillCountText.rectTransform.localScale;

        if (resultWeaponSlots != null)
        {
            _weaponOriginalScales = new Vector3[resultWeaponSlots.Length];
            for (int i = 0; i < resultWeaponSlots.Length; i++)
            {
                _weaponOriginalScales[i] = resultWeaponSlots[i].GetComponent<RectTransform>().localScale;
            }
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
            expSystem.OnLevelUp -= RefreshLevelUI;
        }
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        // 경과 시간을 계속 누적시킵니다.
        elapsedTime += Time.deltaTime;
        UpdateTimerUI();
    }

    #endregion

    // ───────────────────────────────────────────────
    #region Exp / Level UI

    private void RefreshExpUI(float current, float required)
    {
        if (expSlider != null)
        {
            expSlider.minValue = 0f;
            expSlider.maxValue = required;
            expSlider.value = current;
        }
    }

    private void RefreshLevelUI(int newLevel)
    {
        currentLevel = newLevel;

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
        // 남은 시간 계산에서 경과 시간 계산으로 변경
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
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
    private void UpdateCoinUI() => coinCountText.text = $"{coinCount}";

    #endregion

    // ───────────────────────────────────────────────
    #region Button Handlers

    private void OnStopButtonClicked()
    {
        isStopped = !isStopped;
        isTimerRunning = !isStopped;
        Time.timeScale = isStopped ? 0f : 1f;

        var label = stopButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = isStopped ? "▶ 재개" : "⏸ 정지";
    }

    private void OnSettingButtonClicked()
    {
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

    public void ShowGameOver()
    {
        isTimerRunning = false;
        Time.timeScale = 0f;

        UpdateGameOverTexts();
        UpdateGameOverWeapons();

        gameOverPanelAnimator.Show();
        AnimateGameOverElements();
    }

    private void AnimateGameOverElements()
    {
        resultSurvivalTimeText.rectTransform.localScale = Vector3.zero;
        resultLevelText.rectTransform.localScale = Vector3.zero;
        resultKillCountText.rectTransform.localScale = Vector3.zero;

        for (int i = 0; i < resultWeaponSlots.Length; i++)
        {
            resultWeaponSlots[i].GetComponent<RectTransform>().localScale = Vector3.zero;
        }

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        float popDuration = 0.35f;
        float delayBetween = 0.15f;

        seq.AppendInterval(0.3f);

        seq.Append(resultSurvivalTimeText.rectTransform.DOScale(_survivalOriginalScale, popDuration).SetEase(Ease.OutBack));
        seq.AppendInterval(delayBetween);

        seq.Append(resultLevelText.rectTransform.DOScale(_levelOriginalScale, popDuration).SetEase(Ease.OutBack));
        seq.AppendInterval(delayBetween);

        seq.Append(resultKillCountText.rectTransform.DOScale(_killOriginalScale, popDuration).SetEase(Ease.OutBack));
        seq.AppendInterval(delayBetween);

        var currentWeapons = weaponManager.Weapons;
        for (int i = 0; i < resultWeaponSlots.Length; i++)
        {
            if (i < currentWeapons.Count)
            {
                seq.Append(resultWeaponSlots[i].GetComponent<RectTransform>().DOScale(_weaponOriginalScales[i], popDuration).SetEase(Ease.OutBack));
                seq.AppendInterval(0.1f);
            }
            else
            {
                resultWeaponSlots[i].GetComponent<RectTransform>().localScale = _weaponOriginalScales[i];
            }
        }
    }

    private void UpdateGameOverTexts()
    {
        // 생존 시간을 600에서 빼는 대신 경과 시간(elapsedTime)을 그대로 사용
        float survivedTime = elapsedTime;
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

        var currentWeapons = weaponManager.Weapons;

        for (int i = 0; i < resultWeaponSlots.Length; i++)
        {
            if (i < currentWeapons.Count)
            {
                WeaponBase weapon = currentWeapons[i];
                Sprite aura = GetAuraByRarity(weapon.currentRarity);

                resultWeaponSlots[i].UpdateSlot(weapon.WeaponData.weaponIcon, aura, weapon.currentUpgradeLevel);
            }
            else
            {
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