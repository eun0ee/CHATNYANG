using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject levelUpPanel;
    public Image catImage;
    public TMP_InputField playerInputField;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI dealerDialogueText;
    public GameObject resultDisplay;
    public TextMeshProUGUI resultText;
    public Button closeButton;

    [Header("Level Up Settings")]
    public float levelUpTimeout = 20f;
    public int maxCharacterLimit = 5;

    [Header("Weapon Prefabs")]
    public GameObject catnipWeaponPrefab;
    public GameObject bentoniteWeaponPrefab;
    public GameObject furBrushWeaponPrefab;
    public GameObject laserWeaponPrefab;

    [Header("API Connection")]
    public GeminiClient geminiClient;

    private GameObject _player;
    private PlayerStats _playerStats;
    private WeaponManager _weaponManager;
    private ExperienceSystem _expSystem;

    private float _timer;
    private bool _isWaitingForInput = false;
    private Coroutine _countdownCoroutine;

    [System.Serializable]
    public class DealerResponse
    {
        public string item_tag;
        public string dialogue;
    }

    private void Start()
    {
        _player = GameObject.FindWithTag("Player");
        if (_player != null)
        {
            _playerStats = _player.GetComponent<PlayerStats>();
            _weaponManager = _player.GetComponentInChildren<WeaponManager>();
        }

        if (closeButton != null) closeButton.onClick.AddListener(ResumeGame);

        if (playerInputField != null)
        {
            playerInputField.characterLimit = 0;
            playerInputField.onValueChanged.AddListener(TruncateKoreanInput);

            // 엔터키 입력을 감지하는 유니티 내장 이벤트 연결 (마우스 클릭 대체)
            playerInputField.onSubmit.AddListener(OnInputFieldSubmit);
        }

        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        _expSystem = FindObjectOfType<ExperienceSystem>();
        if (_expSystem != null)
        {
            _expSystem.OnLevelUp += TriggerLevelUp;
        }
    }

    private void OnDestroy()
    {
        if (_expSystem != null)
        {
            _expSystem.OnLevelUp -= TriggerLevelUp;
        }
    }

    private void TruncateKoreanInput(string input)
    {
        if (input.Length > maxCharacterLimit)
        {
            playerInputField.text = input.Substring(0, maxCharacterLimit);
            playerInputField.caretPosition = maxCharacterLimit;
        }
    }

    // 인풋필드에서 엔터키가 눌렸을 때 자동으로 호출되는 함수
    private void OnInputFieldSubmit(string text)
    {
        // 대기 중이고 입력된 글자가 한 글자라도 있을 때만 작동
        if (_isWaitingForInput && text.Length > 0)
        {
            SubmitRequest(text);
        }
        else if (_isWaitingForInput && text.Length == 0)
        {
            // 빈 칸으로 엔터를 누르면 입력을 강제로 다시 유도
            playerInputField.ActivateInputField();
        }
    }

    [ContextMenu("Trigger Level Up (Test)")]
    public void TriggerLevelUp()
    {
        Time.timeScale = 0f;

        if (levelUpPanel != null) levelUpPanel.SetActive(true);
        if (resultDisplay != null) resultDisplay.SetActive(false);
        if (closeButton != null) closeButton.gameObject.SetActive(false);

        if (playerInputField != null)
        {
            playerInputField.text = "";
            playerInputField.interactable = true;
            playerInputField.ActivateInputField();
        }

        if (dealerDialogueText != null)
        {
            dealerDialogueText.text = $"원하는 것을 {maxCharacterLimit}글자 내외로 적고 [Enter]를 누르라냥.";
        }

        _timer = levelUpTimeout;
        _isWaitingForInput = true;

        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
        }

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

        // 시간 초과 시 무조건 강제 제출 활성화
        if (_isWaitingForInput)
        {
            // 글자를 적다 말았으면 적힌 글자로, 아예 안 적었으면 타임아웃 문자로 제출
            string finalInput = (playerInputField != null && playerInputField.text.Length > 0) ? playerInputField.text : "타임아웃";
            SubmitRequest(finalInput);
        }
    }

    private void SubmitRequest(string playerText)
    {
        _isWaitingForInput = false;
        if (playerInputField != null) playerInputField.interactable = false;

        if (timerText != null) timerText.text = "분석 중...";
        if (dealerDialogueText != null) dealerDialogueText.text = "하늘에 기도를 전달하는 중이다냥. 기다려라...";

        if (geminiClient != null)
        {
            StartCoroutine(geminiClient.CallGemini(playerText, ProcessAIResponse));
        }
    }

    private void ProcessAIResponse(string rawJson)
    {
        if (string.IsNullOrEmpty(rawJson))
        {
            if (dealerDialogueText != null) dealerDialogueText.text = "통신 오류다냥! 기도가 흐려졌으니 치료 물약이나 먹어라.";
            ApplyRewardEffect("Heal_Potion");
            ShowResultUI();
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
            if (dealerDialogueText != null) dealerDialogueText.text = "알아들을 수 없는 신성모독이다냥! 벌로 돌멩이나 받아라.";
            ApplyRewardEffect("Trash_Item");
        }

        ShowResultUI();
    }

    private void ApplyRewardEffect(string itemTag)
    {
        if (_player == null) _player = GameObject.FindWithTag("Player");
        if (_playerStats == null && _player != null) _playerStats = _player.GetComponent<PlayerStats>();
        if (_weaponManager == null && _player != null) _weaponManager = _player.GetComponentInChildren<WeaponManager>();

        string rewardName = "평범한 돌멩이";
        bool isWeaponGiven = false;

        switch (itemTag)
        {
            case "Weapon_Catnip":
                rewardName = "캣닢 구슬 무기";
                isWeaponGiven = GiveWeaponToManager(catnipWeaponPrefab);
                break;
            case "Weapon_Bentonite":
                rewardName = "벤토나이트 모래 무기";
                isWeaponGiven = GiveWeaponToManager(bentoniteWeaponPrefab);
                break;
            case "Weapon_FurBrush":
                rewardName = "털뿜뿜 브러시 무기";
                isWeaponGiven = GiveWeaponToManager(furBrushWeaponPrefab);
                break;
            case "Weapon_Laser":
                rewardName = "자동 레이저 포인터 무기";
                isWeaponGiven = GiveWeaponToManager(laserWeaponPrefab);
                break;
            case "Heal_Potion":
                rewardName = "생명력 회복 포션";
                if (_playerStats != null)
                {
                    _playerStats.currentHp = Mathf.Min(_playerStats.currentHp + 30f, _playerStats.maxHp);
                    isWeaponGiven = true;
                }
                break;
            case "Trash_Item":
            default:
                rewardName = "평범한 돌멩이 (쓰레기)";
                isWeaponGiven = true;
                break;
        }

        if (!isWeaponGiven && itemTag.StartsWith("Weapon_"))
        {
            rewardName = "무기 슬롯 초과 (또는 이미 보유중)";
            dealerDialogueText.text = "이미 가졌거나 더 들 수 없다냥! 물약이나 먹어라.";
            if (_playerStats != null) _playerStats.currentHp = Mathf.Min(_playerStats.currentHp + 30f, _playerStats.maxHp);
        }

        if (resultText != null)
        {
            resultText.text = $"[ {rewardName} ] 하사 완료!";
        }
    }

    private bool GiveWeaponToManager(GameObject weaponPrefab)
    {
        if (_weaponManager != null && weaponPrefab != null)
        {
            return _weaponManager.AddWeapon(weaponPrefab);
        }
        return false;
    }

    private void ShowResultUI()
    {
        if (resultDisplay != null) resultDisplay.SetActive(true);
        if (closeButton != null) closeButton.gameObject.SetActive(true);
    }

    private void ResumeGame()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        // Time.scale을 Time.timeScale로 수정
        Time.timeScale = 1.0f;
    }
}