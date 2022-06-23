using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public string roleName;
    public string color;
    public int spawnIndex;
    private PhotonView _pv;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (_pv.IsMine)
            // Get local player role
            _pv.RPC(nameof(RPC_GetRole), RpcTarget.MasterClient); // Send PRC_GetRole to this object on the Master Client
    }

    private void CreateController()
    {
        if (roleName != null)
        {
            GameObject village = Map.FindMap();
            Transform spawnList = village.transform.Find("spawns");
            GameObject spawn = spawnList.GetChild(spawnIndex).gameObject;
            Vector3 spawnPoint = spawn.transform.position;

            object[] instantiationData =
                {roleName, color, PhotonNetwork.LocalPlayer.NickName, PhotonNetwork.LocalPlayer.UserId};
            GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), spawnPoint,
                Quaternion.identity, 0, instantiationData);
            MainGameMenuManager.Instance.playerInput = player.GetComponent<PlayerInput>();
            MainGameMenuManager.Instance.loadingScreen.SetActive(false);
        }
        else
        {
            Debug.Log("[-] roleName not set in CreateController, theoretically impossible");
        }
    }

    private void DisplayRole() // Display role name when set
    {
        if (_pv.IsMine)
        {
            RoomManager.Instance.DisplayRole(roleName, RoomManager.Instance.ColorsDict[color]);
            CreateController(); // Call CreateController only when roleName have been received
        }
    }

    [PunRPC]
    private void RPC_GetRole() // Only the Master Client received this RPC
    {
        roleName = RoomManager.Instance.GetNextRoleName(); // Get the role from RoomManager
        color = RoomManager.Instance.GetNextColor();
        spawnIndex = RoomManager.Instance.nextPlayerRoleIndex;
        RoomManager.Instance.nextPlayerRoleIndex++;
        _pv.RPC(nameof(RPC_ReceiveRole), RpcTarget.OthersBuffered, roleName, color,
            spawnIndex); // Broadcast the new role and color
        DisplayRole();
    }

    [PunRPC]
    private void
        RPC_ReceiveRole(string roleName, string color, int spawnIndex) // Apply the role that have been broadcast
    {
        this.roleName = roleName;
        this.color = color;
        this.spawnIndex = spawnIndex;
        DisplayRole();
    }
}