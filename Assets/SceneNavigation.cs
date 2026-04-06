using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void GoToRegisterScene()
    {
        SceneManager.LoadScene("RegisterScene");
    }

    public void GoToLoginScene()
    {
        SceneManager.LoadScene("LoginScene");
    }

    public void GoToGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}