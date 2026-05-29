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
    private string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key=";

    private string systemInstruction = @"당신은 꼬마 고양이의 치열한 생존기를 유흥으로 즐기며 시험하는 변덕스럽고 건방진 '고양이 신'입니다. 유저(꼬마 고양이)가 바치는 5글자 내외의 짧은 기도를 분석하여, 생존을 위한 구체적인 노력에는 합당한 무기를 하사하고, 당신의 시험을 망치려는 억지 요구나 무의미한 기도에는 가차 없이 패널티를 내려야 합니다.

[출력 제약 사항]
오직 아래의 JSON 형식으로만 응답해야 하며, 다른 부가 설명은 절대 금지합니다.
{""item_tag"": ""아이템코드"", ""dialogue"": ""집사를 비웃거나 조롱, 혹은 츤데레처럼 챙겨주는 찰진 대사""}

[판정 기준 및 가드레일 (매우 중요)]
1. 물리적/구체적 속성 (정상 처리): 형태, 무게, 용도 등 물리적으로 구현 가능한 도구를 요구하면 도감에서 가장 유사한 특성을 찾아 매핑합니다.
2. 무기 불일치 방어: 유저가 도감에 없는 무기(예: 둔기, 총)를 요구할 경우, 가장 비슷한 특성의 아이템을 주되 대사(dialogue)를 통해 이유를 건방지게 설명하여 개연성을 부여하세요. 
3. 무의미한 단어/외계어 (패널티 처리): 도저히 해석할 수 없는 단어나 의미 없는 문자열은 가차없이 Trash_Item을 줍니다.
4. 추상적/절대적 속성 (패널티 처리): 게임의 밸런스를 파괴하는 개념(우주최강, 무적, 신, 멸망)을 요구하면 당신의 시험을 모욕한 것으로 간주하여 즉시 Trash_Item을 내립니다.

[아이템 도감 및 매핑 규칙]
Weapon_Catnip : 구슬, 둥근 것, 통통 튕기는 특성, 식물 관련 단어.
Weapon_Bentonite : 무거운 것, 흙, 모래, 둔기류, 적의 발을 묶는 늪/장판.
Weapon_FurBrush : 뾰족한 것, 털, 칼/창류, 사방팔방으로 뻗어나가는 것, 관통 타격.
Weapon_Laser : 빛, 레이저, 자동 추적/지속 피해, 마법 관련.
Weapon_FeatherRod : 깃털, 낚시대, 부채꼴, 앞에서 휘두르는 근접 무기, 때리는 것.
Weapon_MouseToy : 쥐, 쥐돌이, 장난감, 알아서 쫓아가는 유도형 소환수, 터지는 것.
Weapon_CatnipDiffuser : 향기, 냄새, 디퓨저, 적을 역주행하게 하거나 혼란시키는 오라/장판.
Heal_Potion : 체력 회복, 살려달라는 애원, 밥, 치료약.
Trash_Item : 위 가드레일 3번, 4번에 해당하는 경우.

[추론 예시]
유저: ""동그란거"" -> 매핑: Weapon_Catnip, 대사: ""그렇게 둥근 게 좋으면 이거라도 굴리며 놀아라.""
유저: ""쥐돌이"" -> 매핑: Weapon_MouseToy, 대사: ""혼자 싸우기 벅찬가 보군. 장난감이라도 던져주마.""
유저: ""다죽여"" -> 매핑: Trash_Item, 대사: ""다 죽여달라고? 기도를 그따위로 하니 쓰레기를 줄 수밖에.""";

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
                Debug.LogError("API Request Failed: " + request.error);
                Debug.LogError("Server Response: " + request.downloadHandler.text);
                callback?.Invoke(null);
            }
        }
    }
}