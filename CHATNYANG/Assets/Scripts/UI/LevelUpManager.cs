using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class LevelUpManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject        levelUpPanel;
    public TMP_InputField    playerInputField;
    public TextMeshProUGUI   timerText;
    public TextMeshProUGUI   dealerDialogueText;
    public GameObject        resultDisplay;
    public TextMeshProUGUI   resultText;
    public Button            closeButton;

    [Header("Level Up Settings")]
    public float levelUpTimeout    = 20f;
    public int   maxCharacterLimit = 5;

    [Header("Weapon Prefabs")]
    public GameObject catnipWeaponPrefab;
    public GameObject bentoniteWeaponPrefab;
    public GameObject furBrushWeaponPrefab;
    public GameObject laserWeaponPrefab;

    [Header("API Connection")]
    public GeminiClient geminiClient;

    // ── 내부 참조 ──────────────────────────────────
    private GameObject       _player;
    private PlayerStats      _playerStats;
    private WeaponManager    _weaponManager;
    private ExperienceSystem _expSystem;

    private float    _timer;
    private bool     _isWaitingForInput;
    private Coroutine _countdownCoroutine;
    private string   _lastPlayerInput = "";

    [System.Serializable]
    public class DealerResponse
    {
        public string item_tag;
        public string dialogue;
    }

    // ───────────────────────────────────────────────
    private void Start()
    {
        _player = GameObject.FindWithTag("Player");
        if (_player != null)
        {
            _playerStats   = _player.GetComponent<PlayerStats>();
            _weaponManager = _player.GetComponentInChildren<WeaponManager>();
        }

        closeButton?.onClick.AddListener(ResumeGame);

        if (playerInputField != null)
        {
            playerInputField.characterLimit = 0;
            playerInputField.onValueChanged.AddListener(TruncateKoreanInput);
            playerInputField.onSubmit.AddListener(OnInputFieldSubmit);
        }

        levelUpPanel?.SetActive(false);

        _expSystem = FindObjectOfType<ExperienceSystem>();
        if (_expSystem != null)
            _expSystem.OnLevelUp += TriggerLevelUp;
    }

    private void OnDestroy()
    {
        if (_expSystem != null)
            _expSystem.OnLevelUp -= TriggerLevelUp;
    }

    // ── Input ──────────────────────────────────────
    private void TruncateKoreanInput(string input)
    {
        if (input.Length <= maxCharacterLimit) return;
        playerInputField.text          = input.Substring(0, maxCharacterLimit);
        playerInputField.caretPosition = maxCharacterLimit;
    }

    private void OnInputFieldSubmit(string text)
    {
        if (!_isWaitingForInput) return;
        if (text.Length > 0) SubmitRequest(text);
        else playerInputField.ActivateInputField();
    }

    // ── Level Up 진입 ──────────────────────────────
    [ContextMenu("Trigger Level Up (Test)")]
    public void TriggerLevelUp(int level)
    {
        Time.timeScale = 0f;

        levelUpPanel?.SetActive(true);
        resultDisplay?.SetActive(false);
        closeButton?.gameObject.SetActive(false);

        if (playerInputField != null)
        {
            playerInputField.text         = "";
            playerInputField.interactable = true;
            playerInputField.ActivateInputField();
        }

        if (dealerDialogueText != null)
            dealerDialogueText.text = $"원하는 것을 {maxCharacterLimit}글자 이내로 말해 [Enter]를 눌러주세요.";

        _timer             = levelUpTimeout;
        _isWaitingForInput = true;

        if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
        _countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        while (_timer > 0 && _isWaitingForInput)
        {
            _timer -= Time.unscaledDeltaTime;
            if (timerText != null) timerText.text = _timer.ToString("F1") + "초";
            yield return null;
        }

        if (_isWaitingForInput)
        {
            string finalInput = (playerInputField != null && playerInputField.text.Length > 0)
                ? playerInputField.text
                : "타임아웃";
            SubmitRequest(finalInput);
        }
    }

    // ── Submit / Filter ────────────────────────────
    private void SubmitRequest(string playerText)
    {
        _isWaitingForInput = false;
        if (playerInputField != null) playerInputField.interactable = false;

        // 필터 1: 빈 입력 or 타임아웃
        if (string.IsNullOrWhiteSpace(playerText) || playerText == "타임아웃")
        {
            Finish("기도도 안 한다는 거냐, 시간도 없다니! 포션이나 받아.", "Heal_Potion");
            return;
        }
        // 필터 2: 직전과 동일한 입력
        if (playerText == _lastPlayerInput)
        {
            Finish("또 같은 말이라 안 받아준다! 성의가 없으면 포션이나 받아라.", "Heal_Potion");
            return;
        }
        // 필터 3: 초성·자음만 입력
        if (Regex.IsMatch(playerText, "[ㄱ-ㅎㅏ-ㅣ]"))
        {
            Finish("초성이나 자음 단독은 말이 안 된다는 뜻이냐! 다시 와.", "Trash_Item");
            return;
        }

        _lastPlayerInput = playerText;

        if (timerText != null)         timerText.text         = "분석 중...";
        if (dealerDialogueText != null) dealerDialogueText.text = "하늘에 기도를 전달하는 중이다. 기다려라...";

        if (geminiClient != null)
            StartCoroutine(geminiClient.CallGemini(playerText, ProcessAIResponse));
    }

    // 필터 결과를 한 곳에서 처리
    private void Finish(string message, string itemTag)
    {
        if (timerText != null)          timerText.text          = "처리 완료";
        if (dealerDialogueText != null) dealerDialogueText.text = message;
        ApplyRewardEffect(itemTag);
        ShowResultUI();
    }

    // ── AI 응답 ────────────────────────────────────
    private void ProcessAIResponse(string rawJson)
    {
        if (string.IsNullOrEmpty(rawJson))
        {
            Finish("응답 없다! 기도를 못 들었으니 치유 포션이나 받아.", "Heal_Potion");
            return;
        }

        string cleanJson = rawJson.Replace("```json", "").Replace("```", "").Trim();
        try
        {
            DealerResponse response = JsonUtility.FromJson<DealerResponse>(cleanJson);
            if (dealerDialogueText != null) dealerDialogueText.text = response.dialogue;
            ApplyRewardEffect(response.item_tag);
        }
        catch (System.Exception e)
        {
            Debug.LogError("JSON Parsing Error: " + e.Message);
            Finish("알아들을 수 없는 신탁이다! 쓰레기나 받아라.", "Trash_Item");
            return;
        }

        ShowResultUI();
    }

    // ── 보상 적용 ──────────────────────────────────
    private void ApplyRewardEffect(string itemTag)
    {
        // null 방어
        if (_player       == null) _player       = GameObject.FindWithTag("Player");
        if (_playerStats  == null && _player != null) _playerStats  = _player.GetComponent<PlayerStats>();
        if (_weaponManager == null && _player != null) _weaponManager = _player.GetComponentInChildren<WeaponManager>();

        string rewardName  = "알 수 없는 보상";
        bool   rewardGiven = false;

        switch (itemTag)
        {
            case "Weapon_Catnip":
                rewardName  = "캣닢 씨앗 무기";
                rewardGiven = GiveWeapon(catnipWeaponPrefab);
                break;
            case "Weapon_Bentonite":
                rewardName  = "벤토나이트 흙 무기";
                rewardGiven = GiveWeapon(bentoniteWeaponPrefab);
                break;
            case "Weapon_FurBrush":
                rewardName  = "빗살무늬 브러쉬 무기";
                rewardGiven = GiveWeapon(furBrushWeaponPrefab);
                break;
            case "Weapon_Laser":
                rewardName  = "자동 레이저 포인터 무기";
                rewardGiven = GiveWeapon(laserWeaponPrefab);
                break;
            case "Heal_Potion":
                rewardName = "회복 포션";
                if (_playerStats != null)
                {
                    _playerStats.currentHp = Mathf.Min(_playerStats.currentHp + 30f, _playerStats.maxHp);
                    rewardGiven = true;
                }
                break;
            case "Trash_Item":
            default:
                rewardName  = "쓸모없는 쓰레기 (패널티)";
                rewardGiven = true;
                break;
        }

        // 무기 슬롯 초과 또는 중복 시 대체 보상
        if (!rewardGiven && itemTag.StartsWith("Weapon_"))
        {
            rewardName = "무기 추가 불가 (슬롯 초과 또는 중복)";
            if (dealerDialogueText != null)
                dealerDialogueText.text = "이미 있거나 더 이상 들 수 없다! 포션이나 받아.";
            if (_playerStats != null)
                _playerStats.currentHp = Mathf.Min(_playerStats.currentHp + 30f, _playerStats.maxHp);
        }

        if (resultText != null)
            resultText.text = $"[ {rewardName} ] 보상 완료!";
    }

    private bool GiveWeapon(GameObject prefab)
    {
        if (_weaponManager == null || prefab == null) return false;
        return _weaponManager.AddWeapon(prefab);
    }

    private void ShowResultUI()
    {
        resultDisplay?.SetActive(true);
        closeButton?.gameObject.SetActive(true);
    }

    private void ResumeGame()
    {
        levelUpPanel?.SetActive(false);
        Time.timeScale = 1f;
    }
}