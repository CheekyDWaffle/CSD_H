using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuFunction : MonoBehaviour
{

    public GameObject MainMenuFirstButton, optionsFirstButton, closeFirstButton;
    public GameObject OptionsMenu;
    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(MainMenuFirstButton);
    }
    public void QuitGame()
    {
        print("quit");
        Application.Quit();
    }
    public void LoadScene(int targetSceneIndex)
    {
        SceneManager.LoadScene(targetSceneIndex);
    }
    public void openOptions()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsFirstButton);
    }
    public void closeOptions()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(closeFirstButton);

    }
}
