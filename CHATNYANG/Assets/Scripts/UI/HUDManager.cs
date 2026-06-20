using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Audio; // 오디오 관련 네임스페이스 추가

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
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
    [SerializeField] private Button settingCloseButton; // 설정창 내부의 닫기 버튼용
    private bool isStopped = false;

    [Header("Panels")]
    [SerializeField] private PanelAnimator settingPanelAnimator;
    [SerializeField] private PanelAnimator dimBackgroundAnimator; // 배경 어둡게 하는 패널용

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Experience")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private ExperienceSystem expSystem;

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
        if (stopButton != null) stopButton.onClick.AddListener(OnStopButtonClicked);
        if (settingButton != null) settingButton.onClick.AddListener(OnSettingButtonClicked);
        if (titleButton != null) titleButton.onClick.AddListener(OnTitleButtonClicked);
        if (settingCloseButton != null) settingCloseButton.onClick.AddListener(CloseSettingPanel);

        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);

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
        // ESC 키 입력 검사를 시간 흐름(isTimerRunning)보다 위에서 처리하여 멈춰있을 때도 닫히게 함
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnSettingButtonClicked();
        }

        if (!isTimerRunning) return;

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
    #region Timer & Stats

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

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
    #region Settings & Button Handlers

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
        if (settingPanelAnimator == null) return;

        bool open = !settingPanelAnimator.IsVisible;

        if (open)
        {
            isTimerRunning = false;
            Time.timeScale = 0f;
            settingPanelAnimator.Show();
            if (dimBackgroundAnimator != null) dimBackgroundAnimator.Show();
        }
        else
        {
            CloseSettingPanel();
        }
    }

    public void CloseSettingPanel()
    {
        if (settingPanelAnimator == null) return;

        settingPanelAnimator.Hide(() =>
        {
            // 설정창을 닫을 때, 기존에 멈춤(Stop) 버튼을 누른 상태가 아니었다면 시간을 다시 흐르게 함
            if (!isStopped)
            {
                isTimerRunning = true;
                Time.timeScale = 1f;
            }
        });

        if (dimBackgroundAnimator != null) dimBackgroundAnimator.Hide();
    }

    private void SetBGMVolume(float volume)
    {
        float safeVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        if (mainMixer != null) mainMixer.SetFloat("BGMVolume", Mathf.Log10(safeVolume) * 20f);
    }

    private void SetSFXVolume(float volume)
    {
        float safeVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        if (mainMixer != null) mainMixer.SetFloat("SFXVolume", Mathf.Log10(safeVolume) * 20f);
    }

    #endregion

    // ───────────────────────────────────────────────
    #region Game Over

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

    #endregion
}