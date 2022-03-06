using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks // MonoBehaviourPunCallbacks gives access to Photon callbacks
{
    public static Launcher Instance;

    [SerializeField] private TMP_InputField userNameInputField;
    [SerializeField] private GameObject userNameButton;
    [SerializeField] private TMP_InputField roomNameInputField;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private Transform roomListContent;
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private GameObject playerListItemPrefab;
    [SerializeField] private GameObject startGameButton;
    private List<RoomInfo> activeRooms = new List<RoomInfo>();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        MenuManager.Instance.OpenMenu("main");
    }

    // CONNECTION
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        MenuManager.Instance.OpenMenu("title");
    }
    
    // USERNAME
    public void ChangeUsernameInput()
    {
        if (userNameInputField.text.Length >= 3) userNameButton.SetActive(true);
        else userNameButton.SetActive(false);
    }
    
    public void SetUsername()
    {
        PhotonNetwork.NickName = userNameInputField.text;
        Debug.Log("Nickname set to "+ userNameInputField.text);
        MenuManager.Instance.OpenMenu("loading");
        Debug.Log("Connection to Master...");
        PhotonNetwork.ConnectUsingSettings();
    }
    
    // ROOMS
    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameInputField.text)) // Entered room name is not empty
        {
            RoomOptions options = new RoomOptions();
            options.BroadcastPropsChangeToAll = true;
            PhotonNetwork.CreateRoom(roomNameInputField.text, options);
            MenuManager.Instance.OpenMenu("loading");
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu(("loading"));
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom()
    {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        MenuManager.Instance.OpenMenu(("room"));
        
        // Clear the players list
        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }
        
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Count(); i++)
        {
            // Instantiate a playerListItemPrefab in playerListContent
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }
        
        startGameButton.SetActive(PhotonNetwork.IsMasterClient); // startGameButton only active for host
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient); // Activate startGameButton for new host
    }
    
    public void Quit()
    {
        Debug.Log("Leaving Game...");
        Application.Quit();
    }

    // CALLBACKS
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creating Failed: " + message;
        MenuManager.Instance.OpenMenu("error");
    }
    
    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("title");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Update activeRooms list
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList) activeRooms.Remove(room);
            else activeRooms.Add(room);
        }
        
        // Clear the room list
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        // Update roomListContent
        foreach (RoomInfo room in activeRooms)
        {
             // Instantiate a roomListItemPrefab with the correct name
             Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(room);
        }
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }
    
    // GAME
    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1); // Switch scene for all players
    }
}