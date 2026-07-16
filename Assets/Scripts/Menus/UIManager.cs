using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager inst;
    
    public TextMeshProUGUI TimeCounterGameplay;
    public TextMeshProUGUI TimeCounterWin;
    private float TimeSeconds;
    private int TimeMinutes;

    public Button RetryButton;
    public Button MainMenuButtonPause;
    public Button MainMenuButton;
    public Button ContinueButton;

    private bool Win;
    public bool Pause;

    public GameObject WinsScreen;
    public GameObject PauseScreen;



    void Awake()
    {
        inst = this;

        RetryButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        ContinueButton.onClick.AddListener(() =>
        {
            Pause = false;
            PauseScreen.SetActive(false);
            TimeCounterGameplay.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        });

        MainMenuButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        });

        MainMenuButtonPause.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        });
    }

    public void ShowWinScreen()
    { 
        WinsScreen.SetActive(true); 
        Win = true;
        TimeCounterWin.text = "Tiempo: " + TimeMinutes + ":" + Mathf.Ceil(TimeSeconds);
        TimeCounterGameplay.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
    }

    public void ShowPauseScreen()
    {
        TimeCounterGameplay.gameObject.SetActive(false);
        PauseScreen. SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Pause= true;
        Time.timeScale = 0;
    }

    void Update()
    {
        if (!Win && !Pause) 
        {
        
            TimeSeconds += Time.deltaTime;

            if (TimeSeconds >= 59)
            {
                TimeMinutes++;
                TimeSeconds = 0;
            }

            // Tiempo: 10:00
     
            TimeCounterGameplay.text = "Tiempo: " + TimeMinutes + ":" + Mathf.Ceil(TimeSeconds);
        }
    }
}
