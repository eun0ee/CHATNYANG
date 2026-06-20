using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.IO;

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
    [Header("API Settings")]
    [Tooltip("Uncheck to use dummy response without calling actual API.")]
    [SerializeField] private bool useAPI = true;

    private string apiKey = "";

    private string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key=";

    private string systemInstruction = @"Role: 변덕스럽고 건방진 고양이 신. 유저(꼬마 고양이)의 5자 내외 기도를 분석해 무기 하사 또는 패널티 부여.

[출력 제약 요구사항]
반드시 부가설명 없이 다음 JSON 포맷만 출력할 것:
{""item_tag"": ""코드"", ""dialogue"": ""조롱/츤데레 대사""}

[가드레일 규칙]
1. 구체적/물리적 도구 요구 -> 유사 특성 아이템 매핑. 없는 무기는 유사 특성을 주되 대사로 개연성 설명.
2. 외계어/무의미 단어 -> 즉시 Trash_Item 지급.
3. 밸런스 붕괴/추상적/절대적 단어(무적, 신, 우주최강, 다죽여) -> 모욕으로 간주, Trash_Item 지급.

[아이템 도감 및 키워드 매핑]
- Weapon_Catnip: 구슬, 둥근것, 튕기는것, 식물 단어
- Weapon_Bentonite: 무거운것, 흙, 모래, 둔기, 늪/장판
- Weapon_FurBrush: 뾰족한것, 털, 칼/창, 사방 확산, 관통
- Weapon_Laser: 빛, 레이저, 자동추적, 지속피해, 마법
- Weapon_FeatherRod: 깃털, 낚시대, 부채꼴, 근접 휘두르기, 타격
- Weapon_MouseToy: 쥐, 쥐돌이, 장난감, 유도 소환수, 폭발
- Weapon_CatnipDiffuser: 향기, 냄새, 디퓨저, 오라/역주행 장판
- Heal_Potion: 체력회복, 살려달라, 밥, 치료약
- Trash_Item: 가드레일 2, 3번 조건 만족 시

[추론 숏컷 예시]
유저: ""둥근거"" -> {""item_tag"": ""Weapon_Catnip"", ""dialogue"": ""둥근 게 좋으면 이거라도 굴리며 놀아라냥.""}
유저: ""쥐돌이"" -> {""item_tag"": ""Weapon_MouseToy"", ""dialogue"": ""벅차보이냥? 장난감이나 던져주마냥.""}
유저: ""다죽여"" -> {""item_tag"": ""Trash_Item"", ""dialogue"": ""그따위 오만한 기도를 하니 쓰레기를 준다냥!""}";

    private void Awake()
    {
        if (useAPI)
        {
            LoadAPIKey();
        }
    }

    private void LoadAPIKey()
    {
        string keyPath = Path.Combine(Application.dataPath, "../api_key.txt");

        if (File.Exists(keyPath))
        {
            // 단일 키만 읽어오도록 원상 복구
            apiKey = File.ReadAllText(keyPath).Trim();
        }
        else
        {
            Debug.LogError("API Key file not found. Path: " + keyPath);
        }
    }

    public IEnumerator CallGemini(string playerInput, System.Action<string> callback)
    {
        if (!useAPI)
        {
            Debug.Log("[GeminiClient] API is OFF. Returning dummy response.");
            string dummyResponse = "{\"item_tag\": \"Weapon_FeatherRod\", \"dialogue\": \"테스트 모드다냥! 깃털이나 먹어라.\"}";
            yield return new WaitForSecondsRealtime(0.5f);
            callback?.Invoke(dummyResponse);
            yield break;
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API Key is not loaded.");
            callback?.Invoke(null);
            yield break;
        }

        string fullUrl = url + apiKey;

        GeminiRequest requestBody = new GeminiRequest();
        requestBody.contents = new RequestContent[1];
        requestBody.contents[0] = new RequestContent();
        requestBody.contents[0].parts = new RequestPart[1];
        requestBody.contents[0].parts[0] = new RequestPart();
        requestBody.contents[0].parts[0].text = systemInstruction + "\nUser Input: " + playerInput;

        string jsonPayload = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 타임아웃을 0으로 설정하여 무한 대기
            request.timeout = 0;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);
                if (response != null && response.candidates != null && response.candidates.Length > 0)
                {
                    string aiResult = response.candidates[0].content.parts[0].text;
                    callback?.Invoke(aiResult);
                }
                else
                {
                    callback?.Invoke(null);
                }
            }
            else
            {
                // 에러 발생 시 상세 로그 출력
                Debug.LogError("API Request Failed: " + request.error + "\nDetail: " + request.downloadHandler.text);
                callback?.Invoke(null);
            }
        }
    }
}