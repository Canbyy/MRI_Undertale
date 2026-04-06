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
    public string baseUrl = "http://localhost:5126";

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
        StartCoroutine(LoginCoroutine());
    }

    private IEnumerator LoginCoroutine()
    {
        messageText.text = "Bezig met inloggen...";
        welcomeText.text = "";

        LoginRequest requestData = new LoginRequest
        {
            email = emailInput.text,
            password = passwordInput.text
        };

        string json = JsonUtility.ToJson(requestData);

        using UnityWebRequest request = new UnityWebRequest(baseUrl + "/api/Auth/login", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
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
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            messageText.text = "Login mislukt. Controleer je gegevens.";
        }
    }

    private void Start()
    {
        AutoLoginCheck();
    }

    private void AutoLoginCheck()
    {
        if (PlayerPrefs.HasKey(TokenKey) && PlayerPrefs.HasKey(FirstNameKey))
        {
            string firstName = PlayerPrefs.GetString(FirstNameKey);
            welcomeText.text = "Welkom " + firstName;
            messageText.text = "Je bent al ingelogd.";
        }
        else
        {
            welcomeText.text = "";
            messageText.text = "";
        }
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(FirstNameKey);
        PlayerPrefs.Save();

        welcomeText.text = "";
        messageText.text = "Je bent uitgelogd.";
    }
}