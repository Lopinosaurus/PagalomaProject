using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using MainGame.PlayerScripts.Roles;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    private PhotonView PV;
    public string roleName;
    public string color;
    public int spawnIndex;
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
            GameObject village = Map.FindMap();
            Transform spawnList = village.transform.Find("spawns");
            GameObject spawn = spawnList.GetChild(spawnIndex).gameObject;
            Debug.Log("spawn = "+spawn);
            Vector3 spawnPoint = spawn.transform.position;

            string[] instancitationData = new string[] { roleName, color, PhotonNetwork.LocalPlayer.NickName, PhotonNetwork.LocalPlayer.UserId};
            GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), spawnPoint, Quaternion.identity, 0, instancitationData);
            IGMenuManager.Instance.playerInput = player.GetComponent<PlayerInput>();
            IGMenuManager.Instance.loadingScreen.SetActive(false);
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
        spawnIndex = RoomManager.Instance.nextPlayerRoleIndex;
        RoomManager.Instance.nextPlayerRoleIndex ++;
        PV.RPC("RPC_ReceiveRole", RpcTarget.OthersBuffered, roleName, color, spawnIndex); // Broadcast the new role and color
        DisplayRole();
    }

    [PunRPC]
    void RPC_ReceiveRole(string roleName, string color, int spawnIndex) // Apply the role that have been broadcast
    {
        this.roleName = roleName;
        this.color = color;
        this.spawnIndex = spawnIndex;
        DisplayRole();
    }
}