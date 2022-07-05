using System.IO;
using MainGame;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public string roleName;
    public string color;
    public int spawnIndex;
    private Vector3[] _spawnList;
    private PhotonView _pv;
    private Vector3 _spawnCenter;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
        _spawnCenter = Map.FindVillage().transform.Find("spawnCenter").position;
    }

    private void Start()
    {
        if (_pv.IsMine) 
            // Get local player role
            _pv.RPC(nameof(RPC_GetRole), RpcTarget.MasterClient); // Send PRC_GetRole to this object on the Master Client
    }

    private void CreateController()
    {
        if (null != roleName)
        {
            // Makes the players spawn in circle
            float trigo = Mathf.PI * 0.125f * spawnIndex + spawnIndex % 2 == 0 ? Mathf.PI : 0;
            Vector3 spawnPoint = _spawnCenter
                                 + Vector3.forward * Mathf.Sin(trigo) * 3
                                 + Vector3.right * Mathf.Cos(trigo) * 3;
            
            // Makes the players look at the center of the spawn center
            Quaternion spawnRot = Quaternion.LookRotation(_spawnCenter - spawnPoint);

            object[] instantiationData = {roleName, color, PhotonNetwork.LocalPlayer.NickName, PhotonNetwork.LocalPlayer.UserId};
            GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), spawnPoint, spawnRot, 0, instantiationData);
            MainGameMenuManager.Instance.playerInput = player.GetComponent<PlayerInput>();
            
            // TODO Improve loading screen
            MainGameMenuManager.Instance.loadingScreen.SetActive(false);
            
            // Fake players
            Vector3 fakePlayerSpawnPoint = spawnPoint;
            if (Physics.Raycast(spawnPoint, Vector3.down, out RaycastHit hit, 100, 7)) fakePlayerSpawnPoint = hit.point;

            RoomManager.Instance.fakePlayer = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs", "FakePlayer"),
                fakePlayerSpawnPoint,
                Quaternion.identity,
                0,
                new object[]{});

            // Makes the fake player spawn ahead of the real player
            Transform fakePlayerTransform = RoomManager.Instance.fakePlayer.transform;
            fakePlayerTransform.position += fakePlayerTransform.forward * 30;
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
    private void RPC_ReceiveRole(string roleName, string color, int spawnIndex) // Apply the role that have been broadcast
    {
        this.roleName = roleName;
        this.color = color;
        this.spawnIndex = spawnIndex;
        DisplayRole();
    }
}