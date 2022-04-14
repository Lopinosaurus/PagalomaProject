using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class IGMenuManager : MonoBehaviour
{
    public static IGMenuManager Instance;
    public static bool isPaused = false;
    public GameObject pauseMenu;
    public GameObject voteMenu;
    public PlayerInput playerInput;
    public bool testButton = false;
    public bool testPlayerButton = false;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        pauseMenu.SetActive(false);
        voteMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }

            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        pauseMenu.SetActive(false);
        voteMenu.SetActive(false);
        isPaused = false;
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Player");
        }
    }

    void PauseGame()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        pauseMenu.SetActive(true);
        isPaused = true;
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("UI");
        }
    }

    public void OpenVoteMenu()
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("UI");
        }
        voteMenu.SetActive(true);
    }

    public void Quit()
    {
        Debug.Log("Leaving Game...");
        Application.Quit();
    }
    
    
}


//Note Toggle Sneak : faire un bool toggle actif ou pas, et a chaque appel verif ce bool. Si oui playeraction.togglesneak, sinon playeraction.sneakclassic