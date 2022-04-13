using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class IGMenuManager : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject pauseMenuUI;
    public PlayerController playerController;
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputActionMap actionMapUI;
    private InputActionMap actionMapPlayer;

    void Start()
    {
        Debug.Log("InputActionAsset = " + inputActionAsset);
        Debug.Log("Start Loading Action Maps");
        actionMapUI = inputActionAsset.FindActionMap("UI", true);
        Debug.Log("Loaded InUI Action Map");
        actionMapPlayer = inputActionAsset.FindActionMap("Player", true);
        Debug.Log("Loaded All action maps");
        
        actionMapUI.Enable();
        actionMapPlayer.Enable();
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
        pauseMenuUI.SetActive(false);
        isPaused = false;
        playerController.PlayerInput.currentActionMap = actionMapPlayer;
        Debug.Log("Current Action Map after change on Resume = " + playerController.PlayerInput.currentActionMap);
    }

    void PauseGame()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        pauseMenuUI.SetActive(true);
        isPaused = true;
        playerController.PlayerInput.currentActionMap = actionMapUI;
        Debug.Log("Current Action Map after change on Pause = " + playerController.PlayerInput.currentActionMap);
        //playerController.PlayerInput.SwitchCurrentActionMap("UI");
    }

    public void Quit()
    {
        Debug.Log("Leaving Game...");
        Application.Quit();
    }
}


//Note Toggle Sneak : faire un bool toggle actif ou pas, et a chaque appel verif ce bool. Si oui playeraction.togglesneak, sinon playeraction.sneakclassic