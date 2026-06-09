using UnityEngine;
using UnityEngine.EventSystems;

// UI 요소를 클릭했을 때 발생하는 이벤트를 감지하는 인터페이스 상속
public class UISoundElement : MonoBehaviour, IPointerClickHandler
{
    // 인스펙터에서 어떤 소리를 낼지 바로 고를 수 있게 열어둠
    public SfxType clickSound = SfxType.BtnClick;

    // 해당 UI에 마우스 클릭(또는 터치)이 일어나는 순간 자동 실행됨
    public void OnPointerClick(PointerEventData eventData)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(clickSound);
        }
    }
}