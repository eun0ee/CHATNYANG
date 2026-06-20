using UnityEngine;

public class DisplayManager : MonoBehaviour
{
    private void Start()
    {
        // 저장된 화면 설정 불러오기 (기본값 0 = 창모드, 1 = 전체화면)
        int isFullScreen = PlayerPrefs.GetInt("FullScreenMode", 0);
        ApplyScreenMode(isFullScreen == 1);
    }

    // [전체화면] 버튼에 연결할 함수
    public void SetFullScreen()
    {
        ApplyScreenMode(true);
        PlayerPrefs.SetInt("FullScreenMode", 1);
        PlayerPrefs.Save();
    }

    // [창모드] 버튼에 연결할 함수
    public void SetWindowedMode()
    {
        ApplyScreenMode(false);
        PlayerPrefs.SetInt("FullScreenMode", 0);
        PlayerPrefs.Save();
    }

    private void ApplyScreenMode(bool isFullScreen)
    {
        if (isFullScreen)
        {
            // 현재 모니터의 가장 큰 해상도를 가져와서 꽉 찬 전체화면으로 만듭니다.
            Resolution maxRes = Screen.resolutions[Screen.resolutions.Length - 1];
            Screen.SetResolution(maxRes.width, maxRes.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            // 창모드 전환 시 기본 크기를 16:9 비율인 1280x720으로 맞춥니다.
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        }
    }
}