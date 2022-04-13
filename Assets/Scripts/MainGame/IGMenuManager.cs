using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class IGMenuManager : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject pauseMenuUI;
    public static PlayerController playerController;
    public static bool testButton;

    public static void AssignTestKey()
    {
        playerController.playerControls.UI.Test.started += ctx => testButton = ctx.ReadValueAsButton();
        //playerController.playerControls.UI.test.started += ctx => testButton = ctx.ReadValueAsButton();
    }
    void Update()
    {
        if (testButton) Debug.Log("[+] UI map enabled !");
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
        if (playerController != null)
        {
            playerController.PlayerInput.SwitchCurrentActionMap("Player");
            Debug.Log("Current Action Map after change on Resume = " + playerController.PlayerInput.currentActionMap);
        }
       
    }

    void PauseGame()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        pauseMenuUI.SetActive(true);
        isPaused = true;
        if (playerController != null)
        {
            playerController.PlayerInput.SwitchCurrentActionMap("UI");
            Debug.Log("Current Action Map after change on Pause = " + playerController.PlayerInput.currentActionMap);
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