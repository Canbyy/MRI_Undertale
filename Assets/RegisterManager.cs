using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class RegisterManager : MonoBehaviour
{
    [Header("Parent UI")]
    public TMP_InputField parentFirstNameInput;
    public TMP_InputField parentLastNameInput;
    public TMP_InputField parentEmailInput;
    public TMP_InputField parentPasswordInput;

    [Header("Child UI")]
    public TMP_InputField childFirstNameInput;
    public TMP_InputField childLastNameInput;
    public TMP_InputField childAgeInput;
    public TMP_Dropdown childGenderDropdown;

    [Header("Feedback UI")]
    public TMP_Text messageText;
    public TMP_Text welcomeText;

    [Header("API")]
    public string baseUrl = "https://mrigame-api-h0gkdyh7f7cbh7a2.germanywestcentral-01.azurewebsites.net";

    private const string TokenKey = "auth_token";
    private const string FirstNameKey = "first_name";
    private const string EmailKey = "email";

    [System.Serializable]
    public class RegisterParentRequest
    {
        public string email;
        public string password;
        public string firstName;
        public string lastName;
    }

    [System.Serializable]
    public class CreateChildRequest
    {
        public string firstName;
        public string lastName;
        public int age;
        public string gender;
    }

    [System.Serializable]
    public class AuthResponse
    {
        public string token;
        public string firstName;
        public string email;
    }

    public void OnRegisterButtonClicked()
    {
        StartCoroutine(RegisterAndCreateChildCoroutine());
    }

    private IEnumerator RegisterAndCreateChildCoroutine()
    {
        messageText.text = "Account wordt aangemaakt...";
        welcomeText.text = "";

        if (string.IsNullOrWhiteSpace(parentFirstNameInput.text) ||
            string.IsNullOrWhiteSpace(parentLastNameInput.text) ||
            string.IsNullOrWhiteSpace(parentEmailInput.text) ||
            string.IsNullOrWhiteSpace(parentPasswordInput.text) ||
            string.IsNullOrWhiteSpace(childFirstNameInput.text) ||
            string.IsNullOrWhiteSpace(childLastNameInput.text) ||
            string.IsNullOrWhiteSpace(childAgeInput.text))
        {
            messageText.text = "Vul alle velden in.";
            yield break;
        }

        if (!int.TryParse(childAgeInput.text, out int parsedAge))
        {
            messageText.text = "Leeftijd moet een getal zijn.";
            yield break;
        }

        string selectedGender = childGenderDropdown.options[childGenderDropdown.value].text;

        RegisterParentRequest parentRequest = new RegisterParentRequest
        {
            firstName = parentFirstNameInput.text,
            lastName = parentLastNameInput.text,
            email = parentEmailInput.text,
            password = parentPasswordInput.text
        };

        string parentJson = JsonUtility.ToJson(parentRequest);

        using UnityWebRequest registerRequest = new UnityWebRequest(baseUrl + "/api/Auth/register-parent", "POST");
        byte[] parentBody = Encoding.UTF8.GetBytes(parentJson);
        registerRequest.uploadHandler = new UploadHandlerRaw(parentBody);
        registerRequest.downloadHandler = new DownloadHandlerBuffer();
        registerRequest.SetRequestHeader("Content-Type", "application/json");

        yield return registerRequest.SendWebRequest();

        if (registerRequest.result != UnityWebRequest.Result.Success)
        {
            messageText.text = "Registratie mislukt: " + registerRequest.error;
            yield break;
        }

        if (registerRequest.responseCode != 200)
        {
            messageText.text = "Ouder account maken mislukt.";
            yield break;
        }

        string registerResponseJson = registerRequest.downloadHandler.text;
        AuthResponse authResponse = JsonUtility.FromJson<AuthResponse>(registerResponseJson);

        PlayerPrefs.SetString(TokenKey, authResponse.token);
        PlayerPrefs.SetString(FirstNameKey, authResponse.firstName);
        PlayerPrefs.SetString(EmailKey, authResponse.email);
        PlayerPrefs.Save();

        messageText.text = "Ouder account gemaakt. Kind profiel wordt aangemaakt...";

        CreateChildRequest childRequest = new CreateChildRequest
        {
            firstName = childFirstNameInput.text,
            lastName = childLastNameInput.text,
            age = parsedAge,
            gender = selectedGender
        };

        string childJson = JsonUtility.ToJson(childRequest);

        using UnityWebRequest childRequestWeb = new UnityWebRequest(baseUrl + "/api/Children", "POST");
        byte[] childBody = Encoding.UTF8.GetBytes(childJson);
        childRequestWeb.uploadHandler = new UploadHandlerRaw(childBody);
        childRequestWeb.downloadHandler = new DownloadHandlerBuffer();
        childRequestWeb.SetRequestHeader("Content-Type", "application/json");
        childRequestWeb.SetRequestHeader("Authorization", "Bearer " + authResponse.token);

        yield return childRequestWeb.SendWebRequest();

        if (childRequestWeb.result != UnityWebRequest.Result.Success)
        {
            messageText.text = "Ouder account is gemaakt, maar kind profiel niet: " + childRequestWeb.error;
            welcomeText.text = "Welkom " + authResponse.firstName;
            yield break;
        }

        if (childRequestWeb.responseCode == 200)
        {
            messageText.text = "Account en kind profiel succesvol aangemaakt.";
            welcomeText.text = "Welkom " + authResponse.firstName;
        }
        else
        {
            messageText.text = "Ouder account gemaakt, maar kind profiel mislukt.";
            welcomeText.text = "Welkom " + authResponse.firstName;
        }
    }
}