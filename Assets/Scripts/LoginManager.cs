using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;
    public TMP_Text welcomeText;

    [Header("API")]
    public string baseUrl = "https://mrigame-api-h0gkdyh7f7cbh7a2.germanywestcentral-01.azurewebsites.net";

    private const string TokenKey = "auth_token";
    private const string FirstNameKey = "first_name";
    private const string EmailKey = "email";

    [System.Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    public class AuthResponse
    {
        public string token;
        public string firstName;
        public string email;
    }

    public void OnLoginButtonClicked()
    {
        Debug.Log("[Login] Knop ingedrukt.");
        StartCoroutine(LoginCoroutine());
    }

    private IEnumerator LoginCoroutine()
    {
        messageText.text = "Bezig met inloggen...";
        welcomeText.text = "";

        Debug.Log("[Login] Starten met inloggen...");
        Debug.Log("[Login] URL: " + baseUrl + "/api/Auth/login");

        LoginRequest requestData = new LoginRequest
        {
            email = emailInput.text,
            password = passwordInput.text
        };

        string json = JsonUtility.ToJson(requestData);
        Debug.Log("[Login] Request body: " + json);

        using UnityWebRequest request = new UnityWebRequest(baseUrl + "/api/Auth/login", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        Debug.Log("[Login] Response code: " + request.responseCode);
        Debug.Log("[Login] Result: " + request.result);
        Debug.Log("[Login] Response body: " + request.downloadHandler.text);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[Login] Error: " + request.error);
            messageText.text = "Login mislukt: " + request.error;
            yield break;
        }

        if (request.responseCode == 200)
        {
            string responseJson = request.downloadHandler.text;
            AuthResponse responseData = JsonUtility.FromJson<AuthResponse>(responseJson);

            PlayerPrefs.SetString(TokenKey, responseData.token);
            PlayerPrefs.SetString(FirstNameKey, responseData.firstName);
            PlayerPrefs.SetString(EmailKey, responseData.email);
            PlayerPrefs.Save();

            messageText.text = "Login gelukt";
            welcomeText.text = "Welkom " + responseData.firstName;

            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            Debug.LogWarning("[Login] Onverwachte response code: " + request.responseCode);
            messageText.text = "Login mislukt. Controleer je gegevens.";
        }
    }

    private void Start()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("[Login] PlayerPrefs gecleared.");
        Debug.Log("[Login] LoginManager gestart. BaseUrl: " + baseUrl);
        AutoLoginCheck();
    }

    private void AutoLoginCheck()
    {
        if (PlayerPrefs.HasKey(TokenKey) && PlayerPrefs.HasKey(FirstNameKey))
        {
            string firstName = PlayerPrefs.GetString(FirstNameKey);
            welcomeText.text = "Welkom " + firstName;
            messageText.text = "Je bent al ingelogd.";
            Debug.Log("[Login] Al ingelogd als: " + firstName);
        }
        else
        {
            welcomeText.text = "";
            messageText.text = "";
            Debug.Log("[Login] Geen opgeslagen login gevonden.");
        }
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(FirstNameKey);
        PlayerPrefs.Save();

        welcomeText.text = "";
        messageText.text = "Je bent uitgelogd.";
        Debug.Log("[Login] Uitgelogd.");
    }
}