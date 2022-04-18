using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    private PhotonView PV;
    public string roleName;
    public string color;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PV.IsMine)
        {
            // Get local player role
            PV.RPC("RPC_GetRole", RpcTarget.MasterClient); // Send PRC_GetRole to this object on the Master Client
        }
    }

    void CreateController()
    {
        if (roleName != null)
        {
            Vector3 spawnPoint = new Vector3(Random.Range (0, 10), 10, Random.Range (0, 10));
            string[] instancitationData = new string[] { roleName, color, PhotonNetwork.LocalPlayer.NickName, PhotonNetwork.LocalPlayer.UserId};
            GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), spawnPoint, Quaternion.identity, 0, instancitationData);
            IGMenuManager.Instance.playerInput = player.GetComponent<PlayerInput>();
        }
        else
        {
            Debug.Log("[-] roleName not set in CreateController, theoretically impossible");
        }
    }

    void DisplayRole() // Display role name when set
    {
        if (PV.IsMine)
        {
            RoomManager.Instance.DisplayRole(roleName);
            CreateController(); // Call CreateController only when roleName have been received
        }
    }

    [PunRPC]
    void RPC_GetRole() // Only the Master Client received this RPC
    {
        roleName = RoomManager.Instance.GetNextRoleName(); // Get the role from RoomManager
        color = RoomManager.Instance.GetNextColor();
        RoomManager.Instance.nextPlayerRoleIndex ++;
        PV.RPC("RPC_ReceiveRole", RpcTarget.OthersBuffered, roleName, color); // Broadcast the new role and color
        DisplayRole();
    }

    [PunRPC]
    void RPC_ReceiveRole(string roleName, string color) // Apply the role that have been broadcast
    {
        this.roleName = roleName;
        this.color = color;
        DisplayRole();
    }
}