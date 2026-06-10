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
        // 하이라이트 초기화
        weaponButton0.GetComponent<Image>().color = _normalColor;
        weaponButton1.GetComponent<Image>().color = _normalColor;

        // 선택된 버튼 강조
        if (index == 0)
        {
            _selectedPrefab = weaponPrefab0;
            weaponButton0.GetComponent<Image>().color = _selectedColor;
        }
        else
        {
            _selectedPrefab = weaponPrefab1;
            weaponButton1.GetComponent<Image>().color = _selectedColor;
        }

        confirmButton.interactable = true;
    }

    private void OnConfirm()
    {
        if (_selectedPrefab == null) return;

        if (WeaponSelectData.Instance != null)
            WeaponSelectData.Instance.SetWeapon(_selectedPrefab);

        titleManager.OnWeaponConfirmed();
    }

    private void OnClose()
    {
        // 선택 초기화 후 패널 닫기
        _selectedPrefab = null;
        weaponButton0.GetComponent<Image>().color = _normalColor;
        weaponButton1.GetComponent<Image>().color = _normalColor;
        confirmButton.interactable = false;

        gameObject.SetActive(false);
    }
}