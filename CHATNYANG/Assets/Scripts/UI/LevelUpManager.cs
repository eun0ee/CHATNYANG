using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class LevelUpManager : MonoBehaviour
{
    [Header("Main Panel UI")]
    public GameObject levelUpPanel;
    public TMP_InputField playerInputField;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI dealerDialogueText;
    public Button levelUpCloseButton;

    [Header("Result Panel UI")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Image itemIconImage;
    public Image rarityAuraImage;
    public Button resultCloseButton;

    [Header("Level Up Settings")]
    public float levelUpTimeout = 20f;
    public int maxCharacterLimit = 5;

    [Header("Weapon Prefabs")]
    public GameObject catnipWeaponPrefab;
    public GameObject bentoniteWeaponPrefab;
    public GameObject furBrushWeaponPrefab;
    public GameObject laserWeaponPrefab;
    public GameObject featherRodWeaponPrefab;
    public GameObject mouseToyWeaponPrefab;
    public GameObject catnipDiffuserWeaponPrefab;

    [Header("Item Icon Sprites")]
    public Sprite catnipIcon;
    public Sprite bentoniteIcon;
    public Sprite furBrushIcon;
    public Sprite laserIcon;
    public Sprite featherRodIcon;
    public Sprite mouseToyIcon;
    public Sprite catnipDiffuserIcon;
    public Sprite potionIcon;
    public Sprite trashIcon;

    [Header("Rarity Aura Sprites")]
    public Sprite normalAura;
    public Sprite rareAura;
    public Sprite epicAura;
    public Sprite uniqueAura;
    public Sprite legendaryAura;

    [Header("API Connection")]
    public GeminiClient geminiClient;

    private GameObject _player;
    private PlayerStats _playerStats;
    private WeaponManager _weaponManager;
    private ExperienceSystem _expSystem;

    private float _timer;
    private bool _isWaitingForInput;
    private Coroutine _countdownCoroutine;
    private string _lastPlayerInput = "";

    [System.Serializable]
    public class DealerResponse
    {
        public string item_tag;
        public string rarity; // 제미나이 응답에서 등급을 받기 위해 추가
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

        // 두 패널의 닫기 버튼 모두 게임 재개 함수로 연결
        levelUpCloseButton?.onClick.AddListener(ResumeGame);
        resultCloseButton?.onClick.AddListener(ResumeGame);

        if (playerInputField != null)
        {
            playerInputField.characterLimit = 0;
            playerInputField.onValueChanged.AddListener(TruncateKoreanInput);
            playerInputField.onSubmit.AddListener(OnInputFieldSubmit);
        }

        levelUpPanel?.SetActive(false);
        resultPanel?.SetActive(false);

        _expSystem = FindObjectOfType<ExperienceSystem>();
        if (_expSystem != null)
            _expSystem.OnLevelUp += TriggerLevelUp;
    }

    private void OnDestroy()
    {
        if (_expSystem != null)
            _expSystem.OnLevelUp -= TriggerLevelUp;
    }

    private void TruncateKoreanInput(string input)
    {
        if (input.Length <= maxCharacterLimit) return;
        playerInputField.text = input.Substring(0, maxCharacterLimit);
        playerInputField.caretPosition = maxCharacterLimit;
    }

    private void OnInputFieldSubmit(string text)
    {
        if (!_isWaitingForInput) return;
        if (text.Length > 0) SubmitRequest(text);
        else playerInputField.ActivateInputField();
    }

    [ContextMenu("Trigger Level Up (Test)")]
    public void TriggerLevelUp(int level)
    {
        Time.timeScale = 0f;

        levelUpPanel?.SetActive(true);
        resultPanel?.SetActive(false);
        levelUpCloseButton?.gameObject.SetActive(false);

        if (playerInputField != null)
        {
            playerInputField.text = "";
            playerInputField.interactable = true;
            playerInputField.ActivateInputField();
        }

        if (dealerDialogueText != null)
            dealerDialogueText.text = $"원하는 것을 {maxCharacterLimit}글자 이내로 말해 [Enter]를 눌러주세요.";

        _timer = levelUpTimeout;
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

    private void SubmitRequest(string playerText)
    {
        _isWaitingForInput = false;
        if (playerInputField != null) playerInputField.interactable = false;

        if (string.IsNullOrWhiteSpace(playerText) || playerText == "타임아웃")
        {
            Finish("기도도 안 한다는 거냥, 포션이나 받아라냥.", "Heal_Potion", "Normal");
            return;
        }
        if (playerText == _lastPlayerInput)
        {
            Finish("또 같은 말이라 안 받아준다냥! 성의가 없으니 포션이나 받아라냥.", "Heal_Potion", "Normal");
            return;
        }
        if (Regex.IsMatch(playerText, "[ㄱ-ㅎㅏ-ㅣ]"))
        {
            Finish("초성이나 자음 단독은 말이 안 된다냥! 저리가라냥.", "Trash_Item", "Trash");
            return;
        }

        _lastPlayerInput = playerText;

        if (timerText != null) timerText.text = "분석 중...";
        if (dealerDialogueText != null) dealerDialogueText.text = "하늘에 기도를 전달하는 중이다냥. 기다려라냥...";

        if (geminiClient != null)
            StartCoroutine(geminiClient.CallGemini(playerText, ProcessAIResponse));
    }

    private void Finish(string message, string itemTag, string rarity)
    {
        if (timerText != null) timerText.text = "처리 완료";
        if (dealerDialogueText != null) dealerDialogueText.text = message;
        ApplyRewardEffect(itemTag, rarity);
        ShowResultUI();
    }

    private void ProcessAIResponse(string rawJson)
    {
        if (string.IsNullOrEmpty(rawJson))
        {
            Finish("응답 없다냥! 기도를 못 들었으니 치유 포션이나 받아라냥.", "Heal_Potion", "Normal");
            return;
        }

        string cleanJson = rawJson.Replace("```json", "").Replace("```", "").Trim();
        try
        {
            DealerResponse response = JsonUtility.FromJson<DealerResponse>(cleanJson);
            if (dealerDialogueText != null) dealerDialogueText.text = response.dialogue;

            string parsedRarity = string.IsNullOrEmpty(response.rarity) ? "Normal" : response.rarity;
            ApplyRewardEffect(response.item_tag, parsedRarity);
        }
        catch (System.Exception e)
        {
            Debug.LogError("JSON Parsing Error: " + e.Message);
            Finish("알아들을 수 없는 신탁이다냥! 쓰레기나 받아라냥.", "Trash_Item", "Trash");
            return;
        }

        ShowResultUI();
    }

    private void ApplyRewardEffect(string itemTag, string rarity)
    {
        if (_player == null) _player = GameObject.FindWithTag("Player");
        if (_playerStats == null && _player != null) _playerStats = _player.GetComponent<PlayerStats>();
        if (_weaponManager == null && _player != null) _weaponManager = _player.GetComponentInChildren<WeaponManager>();

        string rewardName = "알 수 없는 보상";
        bool rewardGiven = false;
        Sprite selectedIcon = trashIcon;

        switch (itemTag)
        {
            case "Weapon_Catnip":
                rewardName = "캣닢 씨앗";
                selectedIcon = catnipIcon;
                rewardGiven = GiveWeapon(catnipWeaponPrefab);
                break;
            case "Weapon_Bentonite":
                rewardName = "벤토나이트 흙";
                selectedIcon = bentoniteIcon;
                rewardGiven = GiveWeapon(bentoniteWeaponPrefab);
                break;
            case "Weapon_FurBrush":
                rewardName = "빗살무늬 브러쉬";
                selectedIcon = furBrushIcon;
                rewardGiven = GiveWeapon(furBrushWeaponPrefab);
                break;
            case "Weapon_Laser":
                rewardName = "자동 레이저 포인터";
                selectedIcon = laserIcon;
                rewardGiven = GiveWeapon(laserWeaponPrefab);
                break;
            case "Weapon_FeatherRod":
                rewardName = "깃털 낚시대";
                selectedIcon = featherRodIcon;
                rewardGiven = GiveWeapon(featherRodWeaponPrefab);
                break;
            case "Weapon_MouseToy":
                rewardName = "태엽 쥐돌이";
                selectedIcon = mouseToyIcon;
                rewardGiven = GiveWeapon(mouseToyWeaponPrefab);
                break;
            case "Weapon_CatnipDiffuser":
                rewardName = "캣닢 디퓨저";
                selectedIcon = catnipDiffuserIcon;
                rewardGiven = GiveWeapon(catnipDiffuserWeaponPrefab);
                break;
            case "Heal_Potion":
                rewardName = "회복 포션";
                selectedIcon = potionIcon;
                if (_playerStats != null)
                {
                    _playerStats.currentHp = Mathf.Min(_playerStats.currentHp + 30f, _playerStats.maxHp);
                    rewardGiven = true;
                }
                break;
            case "Trash_Item":
            default:
                rewardName = "쓸모없는 쓰레기";
                selectedIcon = trashIcon;
                rewardGiven = true;
                break;
        }

        if (!rewardGiven && itemTag.StartsWith("Weapon_"))
        {
            rewardName = "중복 무기 (대체 포션)";
            selectedIcon = potionIcon;
            if (_playerStats != null)
                _playerStats.currentHp = Mathf.Min(_playerStats.currentHp + 30f, _playerStats.maxHp);
        }

        string rarityName = "노말";
        Sprite selectedAura = normalAura;

        switch (rarity)
        {
            case "Normal": rarityName = "노말"; selectedAura = normalAura; break;
            case "Rare": rarityName = "레어"; selectedAura = rareAura; break;
            case "Epic": rarityName = "에픽"; selectedAura = epicAura; break;
            case "Unique": rarityName = "유니크"; selectedAura = uniqueAura; break;
            case "Legendary": rarityName = "레전더리"; selectedAura = legendaryAura; break;
            case "Trash": rarityName = "꽝"; selectedAura = normalAura; break;
            default: rarityName = rarity; selectedAura = normalAura; break;
        }

        if (resultText != null)
            resultText.text = $"[{rewardName}] ({rarityName})";

        if (itemIconImage != null)
            itemIconImage.sprite = selectedIcon;

        if (rarityAuraImage != null)
            rarityAuraImage.sprite = selectedAura;
    }

    private bool GiveWeapon(GameObject prefab)
    {
        if (_weaponManager == null || prefab == null) return false;
        return _weaponManager.AddWeapon(prefab);
    }

    private void ShowResultUI()
    {
        resultPanel?.SetActive(true);
    }

    private void ResumeGame()
    {
        levelUpPanel?.SetActive(false);
        resultPanel?.SetActive(false);
        Time.timeScale = 1f;
    }
}