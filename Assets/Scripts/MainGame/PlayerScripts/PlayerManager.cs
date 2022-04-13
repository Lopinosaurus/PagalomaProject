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
            Vector3 spawnPoint = new Vector3(Random.Range (0, 10), 1, Random.Range (0, 10));
            GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", roleName), spawnPoint, Quaternion.identity);
            IGMenuManager.Instance.playerInput =  player.GetComponent<PlayerInput>();
            // IGMenuManager.Instance.AssignTestKey();
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
        RoomManager.Instance.nextPlayerRoleIndex ++;
        PV.RPC("RPC_ReceiveRole", RpcTarget.OthersBuffered, roleName); // Broadcast the new role
        DisplayRole();
    }

    [PunRPC]
    void RPC_ReceiveRole(string whichRole) // Apply the role that have been broadcast
    {
        roleName = whichRole;
        DisplayRole();
    }
}