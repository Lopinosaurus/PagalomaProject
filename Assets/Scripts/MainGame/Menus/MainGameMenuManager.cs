using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainGameMenuManager : MonoBehaviour
{
    public static MainGameMenuManager Instance;
    private static bool _isPaused;
    
    // Menu screens
    public GameObject pauseMenu;
    public GameObject optionMenu;
    public GameObject voteMenu;
    public GameObject endScreen;
    public GameObject loadingScreen;
    public GameObject playerClock;
    public TMP_Text victoryText;
    public TMP_Text defeatText;
    public TMP_Text whoWonText;
    
    public PlayerInput playerInput;

    private void Awake() => Instance = this;

    private void Start()
    {
        voteMenu.SetActive(false);
        optionMenu.SetActive(false);
        endScreen.SetActive(false);
        // playerClock.SetActive(false);
        // TODO assign something to this value !!
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void ResumeGame()
    {
        if (optionMenu.activeSelf) optionMenu.SetActive(false);
        else if (pauseMenu.activeSelf) pauseMenu.SetActive(false);
        else voteMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        _isPaused = false;
        if (playerInput) playerInput.SwitchCurrentActionMap("Player");
    }

    private void PauseGame()
    {
        pauseMenu.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _isPaused = true;
        if (playerInput) playerInput.SwitchCurrentActionMap("UI");
    }

    public void OpenVoteMenu()
    {
        voteMenu.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (playerInput) playerInput.SwitchCurrentActionMap("UI");
    }

    public void OpenEndMenu(bool victory, int whoWon)
    {
        endScreen.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        playerInput.enabled = true;
        if (playerInput) playerInput.SwitchCurrentActionMap("UI");
        
        victoryText.enabled = victory;
        defeatText.enabled = !victory;

        whoWonText.text = whoWon switch
        {
            1 => "The Werewolves WON",
            2 => "The Villagers WON",
            _ => "Nobody WON"
        };
    }

    public void Quit()
    {
        // May need to destroy RoomManager
        Debug.Log("Leaving Game...");
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    public void OpenTabMenu()
    {
    }
    
    //Note Toggle Sneak : faire un bool toggle actif ou pas, et a chaque appel verif ce bool. Si oui playeraction.togglesneak, sinon playeraction.sneakclassic
}


