using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    // Go to a new scene
    public void GoToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Close game
    public void QuitApp()
    {
        Application.Quit();
        Debug.Log("Application Quit");
    }
}
