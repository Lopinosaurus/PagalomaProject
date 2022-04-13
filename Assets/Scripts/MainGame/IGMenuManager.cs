using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class IGMenuManager : MonoBehaviour
{
    public static IGMenuManager Instance;
    public static bool isPaused = false;
    public GameObject pauseMenuUI;
    public PlayerInput playerInput;
    public bool testButton = false;
    public bool testPlayerButton = false;

    void Awake()
    {
        Instance = this;
    }
    // public void AssignTestKey()
    // {
    //     playerInput.actions["Test"].started += ctx => testButton = ctx.ReadValueAsButton();
    //     playerInput.actions["Test"].canceled += ctx => testButton = ctx.ReadValueAsButton();
    //     playerInput.actions["TestPlayer"].started += ctx => testPlayerButton = ctx.ReadValueAsButton();
    //     playerInput.actions["TestPlayer"].canceled += ctx => testPlayerButton = ctx.ReadValueAsButton();
    //     //playerInput.UI.Test.started += ctx => testButton = ctx.ReadValueAsButton();
    // }
    void Update()
    {
        // if (testButton) Debug.Log("[+] UI map enabled ! (Test performed)");
        // if (testPlayerButton) Debug.Log("[+] Player map enabled ! (TestPlayer performed)");
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
        pauseMenuUI.SetActive(false);
        isPaused = false;
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Player");
            Debug.Log("Current Action Map after change on Resume = " + playerInput.currentActionMap);
        }
    }

    void PauseGame()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        pauseMenuUI.SetActive(true);
        isPaused = true;
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("UI");
            Debug.Log("Current Action Map after change on Pause = " + playerInput.currentActionMap);
            //playerController.PlayerInput.SwitchCurrentActionMap("UI");
        }
    }

    public void Quit()
    {
        Debug.Log("Leaving Game...");
        Application.Quit();
    }
}


//Note Toggle Sneak : faire un bool toggle actif ou pas, et a chaque appel verif ce bool. Si oui playeraction.togglesneak, sinon playeraction.sneakclassic