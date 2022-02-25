using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Random = UnityEngine.Random;

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
            PV.RPC("PRC_GetRole", RpcTarget.MasterClient); // Send PRC_GetRole to this object on the Master Client
            CreateController();
        }
    }

    void CreateController()
    {
        Vector3 spawnPoint = new Vector3(Random.Range (0, 10), 1, Random.Range (0, 10));
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), spawnPoint, Quaternion.identity);
    }

    [PunRPC]
    void PRC_GetRole() // Only the Master Client received this RPC
    {
        roleName = RoomManager.Instance.GetNextRoleName(); // Get the role from RoomManager
        RoomManager.Instance.nextPlayerRoleIndex ++;
        PV.RPC("PRC_SentRole", RpcTarget.OthersBuffered, roleName); // Broadcast the new role
    }

    [PunRPC]
    void PRC_SentRole(string whichRole) // Apply the role that have been broadcast
    {
        roleName = whichRole;
    }
}