using UnityEngine;
using UnityEngine.SceneManagement;

public class Start_Game : MonoBehaviour
{
    public GameObject startScreen;
    public GameObject instructionScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startScreen.SetActive(true);
        instructionScreen.SetActive(false);
    }

    public void OpenInstructions()
    {
        instructionScreen.SetActive(true);
        startScreen.SetActive(false);
    }

    public void OpenGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName); // Reload the current scene
    }
}
