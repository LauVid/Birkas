using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonMngr : MonoBehaviour
{
    public GameObject HowToPlayPanel;
    public GameObject SettingsPanel;
    public GameObject title;
    public GameMngr gameMngr;
    public GameObject Player;

    public Button Take3cards;
    public Button TakeAllcards;


    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
    {

    }
    public void Take3Cards()
    {
        if(gameMngr.PlayedCards.transform.childCount>1)
        {
            gameMngr.Take3Cards(Player);            
        }
    }
    public void TakeAllCards()
    {
        if(gameMngr.PlayedCards.transform.childCount>1)
        {
            gameMngr.TakeAllCards(Player);           
        }

    }

    public void StartGame()
    {
        SceneManager.LoadScene("Table");
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void QuitGame()
    {
    #if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    public void OpenHowToPlay()
    {
        title.SetActive(false);
        HowToPlayPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        title.SetActive(false);
        SettingsPanel.SetActive(true);
    }

    public void Back()
    {
        HowToPlayPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        title.SetActive(true);
    }

    public void SetFullscreen()
    {
        Debug.Log("Changed fullscreen");
        Screen.fullScreen=!Screen.fullScreen;
    }
}
