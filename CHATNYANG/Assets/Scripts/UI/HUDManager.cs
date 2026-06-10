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
        gameOverPanelAnimator.Show();
    }

    private void OnTitleButtonClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }
}