using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image auraImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image[] pawImages; // 발바닥 이미지 3개 배열

    [Header("Settings")]
    [SerializeField] private Color pawActiveColor = Color.white;
    [SerializeField] private Color pawInactiveColor = new Color(1, 1, 1, 0.2f); // 비활성 시 반투명

    // 슬롯 초기화 (무기가 없을 때)
    public void ClearSlot()
    {
        gameObject.SetActive(false);
    }

    // 무기 정보에 맞춰 슬롯 갱신
    public void UpdateSlot(Sprite icon, Sprite aura, int upgradeLevel)
    {
        gameObject.SetActive(true);
        iconImage.sprite = icon;
        auraImage.sprite = aura;

        // 발바닥 레벨 표시 (0강은 0개, 3강은 3개 불 들어옴)
        for (int i = 0; i < pawImages.Length; i++)
        {
            if (i < upgradeLevel)
            {
                pawImages[i].color = pawActiveColor;
                // 혹은 다른 스프라이트를 넣고 싶다면: pawImages[i].sprite = activePawSprite;
            }
            else
            {
                pawImages[i].color = pawInactiveColor;
            }
        }
    }
}