using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuFunction : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
    }
    public void LoadScene(int targetSceneIndex)
    {
        SceneManager.LoadScene(targetSceneIndex);
    }
}
