using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuFunction : MonoBehaviour
{

    public GameObject MainMenuFirstButton, optionsFirstButton, closeFirstButton, controlsOpen, controlsClose, player2Open,player2Close;
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
    public void OpenControls()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(controlsOpen);
    }
    public void CloseControls()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(controlsClose);
    }
    public void OpenPlayer2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(player2Open);
    }
    public void ClosePlayer2()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(player2Close);
    }

}
