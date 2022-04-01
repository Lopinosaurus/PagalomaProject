using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject menuUI;
    [SerializeField] private PlayerController playerController;
    private PlayerControls playerControls;
    private PlayerControls.PlayerActions playeractions;
    
    // [SerializeField] private InputActionMap inUIActionMap;
    // [SerializeField] private InputActionMap inGameActionMap;

    
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
        Debug.Log("Visible = false");
        Cursor.lockState = CursorLockMode.Locked;
        menuUI.SetActive(false);
        isPaused = false;
        //playerController.PlayerInput.currentActionMap = inGameActionMap;
    }

    void PauseGame()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        menuUI.SetActive(true);
        isPaused = true;

        playeractions.Enable();
        //playerControls.Enable();
        Debug.Log("Current Action Map beforge change = " + playerController.PlayerInput.currentActionMap);
        playerController.PlayerInput.currentActionMap.Enable();
        Debug.Log("Current Action Map after change" + playerController.PlayerInput.currentActionMap);
        
        // Setting first to default actionMap
        //playerController.PlayerInput.currentActionMap = inGameActionMap;
        
        // As the player is in UI, switching to UI mode
        //playerController.PlayerInput.currentActionMap = inUIActionMap;
        
        
    }

    public void Quit()
    {
        Debug.Log("Leaving Game...");
        Application.Quit();
    }
}


// ToDo : Basculer les actions maps dans les serializefields, et essayer de les copier depuis le PlayerControls (type InputActionMap) pour les balancer dans les 2 var inUiActionMap et
// inGameActionMap

//Note Toggle Sneak : faire un bool toggle actif ou pas, et a chaque appel verif ce bool. Si oui playeraction.togglesneak, sinon playeraction.sneakclassic