using UnityEngine;
using UnityEngine.UI;

public class WeaponSelectUI : MonoBehaviour
{
    [Header("무기 버튼 2개 (순서대로 연결)")]
    [SerializeField] private Button weaponButton0; // 캣닙 구슬
    [SerializeField] private Button weaponButton1; // 낚싯대

    [Header("무기 프리팹 (버튼 순서와 동일하게)")]
    [SerializeField] private GameObject weaponPrefab0;
    [SerializeField] private GameObject weaponPrefab1;

    [Header("버튼")]
    [SerializeField] private Button confirmButton; // 게임시작
    [SerializeField] private Button closeButton;   // X 버튼

    [SerializeField] private TitleManager titleManager;

    [Header("무기 버튼 이미지")]
    [SerializeField] private Sprite weaponButton0Normal;    // 버튼0 기본 이미지
    [SerializeField] private Sprite weaponButton0Selected;  // 버튼0 선택 이미지
    [SerializeField] private Sprite weaponButton1Normal;    // 버튼1 기본 이미지
    [SerializeField] private Sprite weaponButton1Selected;  // 버튼1 선택 이미지

    // 선택 강조 색상
    private readonly Color _normalColor   = Color.white;
    private readonly Color _selectedColor = new Color(1f, 0.85f, 0.3f);

    private GameObject _selectedPrefab;

    private void Start()
    {
        confirmButton.interactable = false;

        // 기존 에디터 등록 이벤트 제거 후 추가
        weaponButton0.onClick.RemoveAllListeners();
        weaponButton1.onClick.RemoveAllListeners();
        confirmButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        weaponButton0.onClick.AddListener(() => OnWeaponSelected(0));
        weaponButton1.onClick.AddListener(() => OnWeaponSelected(1));
        confirmButton.onClick.AddListener(OnConfirm);
        closeButton.onClick.AddListener(OnClose);
    }

    private void OnWeaponSelected(int index)
    {
        Debug.Log($"[WeaponSelectUI] OnWeaponSelected 호출 / index: {index}");

        // 두 버튼 모두 기본 이미지로 초기화
        weaponButton0.GetComponent<Image>().sprite = weaponButton0Normal;
        weaponButton1.GetComponent<Image>().sprite = weaponButton1Normal;

        if (index == 0)
        {
            _selectedPrefab = weaponPrefab0;
            weaponButton0.GetComponent<Image>().sprite = weaponButton0Selected;
        }
        else
        {
            _selectedPrefab = weaponPrefab1;
            weaponButton1.GetComponent<Image>().sprite = weaponButton1Selected;
        }

        confirmButton.interactable = true;
    }

    private void OnConfirm()
    {
        Debug.Log($"[WeaponSelectUI] OnConfirm 호출 / 선택된 무기: {(_selectedPrefab != null ? _selectedPrefab.name : "null")}");
    
        if (_selectedPrefab == null) return;

        if (WeaponSelectData.Instance != null)
            WeaponSelectData.Instance.SetWeapon(_selectedPrefab);

        titleManager.OnWeaponConfirmed();
    }

    private void OnClose()
    {
        _selectedPrefab = null;
        weaponButton0.GetComponent<Image>().sprite = weaponButton0Normal;
        weaponButton1.GetComponent<Image>().sprite = weaponButton1Normal;
        confirmButton.interactable = false;

        titleManager.CloseWeaponSelect(); // ← 교체
    }
}