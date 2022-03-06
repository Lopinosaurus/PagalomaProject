using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject menuUI;
    [SerializeField] private PlayerController playerController;

    // Update is called once per frame
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
        playerController.PlayerInput.SwitchCurrentActionMap("Player");
        Cursor.visible = false;
        Debug.Log("Visible = false");
        Cursor.lockState = CursorLockMode.Locked;
        menuUI.SetActive(false);
        isPaused = false;
    }

    void PauseGame()
    {
        playerController.PlayerInput.SwitchCurrentActionMap("UI");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        menuUI.SetActive(true);
        isPaused = true;
    }

    public void Quit()
    {
        Debug.Log("Leaving Game...");
        Application.Quit();
    }
}
