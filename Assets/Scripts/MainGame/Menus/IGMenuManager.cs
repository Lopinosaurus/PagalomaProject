using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MainGame.Menus
{
    public class IGMenuManager : MonoBehaviour
    {
        public static IGMenuManager Instance;
        public static bool isPaused = false;
    
<<<<<<< Updated upstream
    // Menu screens
    public GameObject pauseMenu;
    public GameObject optionMenu;
    public GameObject voteMenu;
    public GameObject loadingScreen;
        
    public PlayerInput playerInput;
=======
        // Menu screens
        public GameObject pauseMenu;
        public GameObject optionMenu;
        public GameObject voteMenu;
        public GameObject endScreen;
        public GameObject loadingScreen;
        public TMP_Text victoryText;
        public TMP_Text defeatText;
        public TMP_Text whoWonText;
    
        public PlayerInput playerInput;
>>>>>>> Stashed changes

        void Awake()
        {
            Instance = this;
        }

<<<<<<< Updated upstream
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
=======
        private void Start()
        {
            voteMenu.SetActive(false);
            optionMenu.SetActive(false);
            voteMenu.SetActive(false);
            endScreen.SetActive(false);
        }

        void Update()
>>>>>>> Stashed changes
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
            if (optionMenu.activeSelf) optionMenu.SetActive(false);
            else if (pauseMenu.activeSelf) pauseMenu.SetActive(false);
            else voteMenu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            isPaused = false;
            if (playerInput != null) playerInput.SwitchCurrentActionMap("Player");
        }

        void PauseGame()
        {
            pauseMenu.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            isPaused = true;
            if (playerInput != null) playerInput.SwitchCurrentActionMap("UI");
        }

        public void OpenVoteMenu()
        {
            voteMenu.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (playerInput != null) playerInput.SwitchCurrentActionMap("UI");
        }

<<<<<<< Updated upstream
    public void Quit()
    {
        // May need to destroy RoomManager
        Debug.Log("Leaving Game...");
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
=======
        public void OpenEndMenu(bool victory, int whoWon)
        {
            endScreen.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (playerInput != null) playerInput.SwitchCurrentActionMap("UI");
        
            victoryText.enabled = victory;
            defeatText.enabled = !victory;

            if (whoWon == 1) whoWonText.text = "The Werewolves WON";
            else if (whoWon == 2) whoWonText.text = "The Villagers WON";
            else whoWonText.text = "Nobody WON";
        }

        public void Quit()
        {
            // May need to destroy RoomManager
            Debug.Log("Leaving Game...");
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene(0);
        }
>>>>>>> Stashed changes
    }
}


//Note Toggle Sneak : faire un bool toggle actif ou pas, et a chaque appel verif ce bool. Si oui playeraction.togglesneak, sinon playeraction.sneakclassic