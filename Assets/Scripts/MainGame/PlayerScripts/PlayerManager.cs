using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Random = UnityEngine.Random;
using TMPro;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    private PhotonView _photonView;
    public string roleName;
    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (_photonView.IsMine)
        {
            // Get local player role
            _photonView.RPC("RPC_GetRole", RpcTarget.MasterClient); // Send PRC_GetRole to this object on the Master Client
        }
    }

    private void CreateController()
    {
        if (roleName != null)
        {
            Vector3 spawnPoint = new Vector3(Random.Range (0, 10), 1, Random.Range (0, 10));

            roleName = "Player"; //TODO EVOLVE

            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", roleName), spawnPoint, Quaternion.identity);
        }
        else
        {
            Debug.Log("[-] roleName not set in CreateController, theoretically impossible");
        }
    }

    private void DisplayRole() // Display role name when set
    {
        if (_photonView.IsMine)
        {
            RoomManager.Instance.DisplayRole(roleName);
            CreateController(); // Call CreateController only when roleName have been received
        }
    }

    [PunRPC]
    private void RPC_GetRole() // Only the Master Client received this RPC
    {
        roleName = RoomManager.Instance.GetNextRoleName(); // Get the role from RoomManager
        RoomManager.Instance.nextPlayerRoleIndex ++;
        _photonView.RPC("RPC_ReceiveRole", RpcTarget.OthersBuffered, roleName); // Broadcast the new role
        DisplayRole();
    }

    [PunRPC]
    private void RPC_ReceiveRole(string whichRole) // Apply the role that have been broadcast
    {
        roleName = whichRole;
        DisplayRole();
    }
    
}