using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

[RequireComponent(typeof(Toggle))]
public class WeaponSoundToggle : MonoBehaviour
{
    [Header("오디오 믹서 설정")]
    public AudioMixer mainMixer;
    public string weaponVolumeParam = "WeaponVolume"; // 믹서에 노출시킬 파라미터 이름

    private Toggle _toggle;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
    }

    private void Start()
    {
        // 1. 기존에 저장된 설정 무시하고 무조건 true(On)로 강제 설정
        _toggle.isOn = true;

        // 2. 오디오 믹서 볼륨도 켜진 상태로 강제 적용
        SetWeaponVolume(true);

        // 3. 토글 버튼 이벤트 연결
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        // 볼륨 적용 및 설정 저장
        SetWeaponVolume(isOn);
        PlayerPrefs.SetInt("WeaponSoundOn", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SetWeaponVolume(bool isOn)
    {
        if (mainMixer != null)
        {
            // isOn이 true면 0dB(원래 소리 크기), false면 -80dB(완전 음소거)
            float volume = isOn ? 0f : -80f;
            mainMixer.SetFloat(weaponVolumeParam, volume);
        }
    }
}