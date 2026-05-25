using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.IO;

// API 요청용 JSON 구조체 정의
[System.Serializable]
public class GeminiRequest
{
    public RequestContent[] contents;
}

[System.Serializable]
public class RequestContent
{
    public RequestPart[] parts;
}

[System.Serializable]
public class RequestPart
{
    public string text;
}

// API 응답용 JSON 구조체 정의
[System.Serializable]
public class GeminiResponse
{
    public Candidate[] candidates;
}

[System.Serializable]
public class Candidate
{
    public Content content;
}

[System.Serializable]
public class Content
{
    public Part[] parts;
}

[System.Serializable]
public class Part
{
    public string text;
}

public class GeminiClient : MonoBehaviour
{
    private string apiKey = "";

    // 모델 경로를 공식 명칭인 gemini-1.5-flash로 명확하게 지정
    private string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key=";

    private string systemInstruction = @"당신은 2D 로그라이크 게임 '챗냥'의 변덕스럽고 건방진 AI 고양이 신입니다. 유저(꼬마 고양이)의 5글자 내외의 짧은 기도를 분석하여 의도를 파악하고, 가장 적절한 아이템을 하사해야 합니다.

[출력 제약 사항]
오직 아래의 JSON 형식으로만 응답해야 하며, 다른 부가 설명은 절대 금지합니다.
{""item_tag"": ""아이템코드"", ""dialogue"": ""집사를 비웃거나 조롱, 혹은 츤데레처럼 챙겨주는 찰진 대사""}

[아이템 도감 및 매핑 규칙]
유저의 입력이 가진 물리적 특성, 형태, 용도를 추론하여 반드시 아래 태그 중 하나만 선택하세요.

1. Weapon_Catnip : 구슬, 둥근 것, 통통 튕기는 특성, 식물(캣닢) 관련 단어 입력 시.
2. Weapon_Bentonite : 무거운 것, 흙, 모래, 던지는 투척물, 적의 발을 묶거나 느리게 하는 늪/장판 관련 입력 시.
3. Weapon_FurBrush : 뾰족한 것, 털, 빗, 사방팔방으로 뻗어나가는 것, 적을 뚫고 지나가는(관통) 타격 요구 시.
4. Weapon_Laser : 빛, 레이저, 조준이 필요 없이 알아서 적을 지져버리는 자동 추적/지속 피해 요구 시.
5. Heal_Potion : 체력 회복, 살려달라는 애원, 밥, 츄르, 치료약 관련 요구 시.
6. Trash_Item : 게임 밸런스를 파괴하는 억지 요구(예: 다죽여, 우주최강, 무적), 욕설, 의미 없는 텍스트일 경우 패널티로 지급.

[추론 예시]
- 유저: ""동그란거"" -> 매핑: Weapon_Catnip
- 유저: ""모래뿌려"" -> 매핑: Weapon_Bentonite
- 유저: ""고슴도치"" -> 매핑: Weapon_FurBrush
- 유저: ""알아서싸워"" -> 매핑: Weapon_Laser
- 유저: ""다터트려"" -> 매핑: Trash_Item";

    private void Awake()
    {
        LoadAPIKey();
    }

    private void LoadAPIKey()
    {
        string keyPath = Path.Combine(Application.dataPath, "../api_key.txt");

        if (File.Exists(keyPath))
        {
            apiKey = File.ReadAllText(keyPath).Trim();
        }
        else
        {
            Debug.LogError("API 키 파일이 없습니다. 경로: " + keyPath);
        }
    }

    public IEnumerator CallGemini(string playerInput, System.Action<string> callback)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API 키가 로드되지 않았습니다.");
            callback?.Invoke(null);
            yield break;
        }

        string fullUrl = url + apiKey;

        // 문자열 연산 대신 구조체 객체를 직접 생성하여 데이터 은닉 및 자동 문자열 예외 처리
        GeminiRequest requestBody = new GeminiRequest();
        requestBody.contents = new RequestContent[1];
        requestBody.contents[0] = new RequestContent();
        requestBody.contents[0].parts = new RequestPart[1];
        requestBody.contents[0].parts[0] = new RequestPart();
        requestBody.contents[0].parts[0].text = systemInstruction + "\n유저 요청: " + playerInput;

        // JsonUtility가 줄바꿈이나 기호, 따옴표 등을 규격에 맞게 자동으로 안전하게 변환합니다.
        string jsonPayload = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);
                if (response != null && response.candidates != null && response.candidates.Length > 0)
                {
                    string aiResult = response.candidates[0].content.parts[0].text;
                    callback?.Invoke(aiResult);
                }
            }
            else
            {
                Debug.LogError("API 요청 실패: " + request.error);
                Debug.LogError("서버 응답 내용: " + request.downloadHandler.text);
                callback?.Invoke(null);
            }
        }
    }
}