using UnityEngine;

public class AppManager : MonoBehaviour
{
    // [게임 종료] 버튼에 연결할 함수
    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다...");

#if UNITY_EDITOR
        // 유니티 에디터 안에서 플레이 중일 때 종료하는 코드
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 실제 빌드된 게임에서 끄는 코드
        Application.Quit();
#endif
    }
}