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
    public List<GameObject> spawns;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PV.IsMine)
        {
            CreateController();
        }
    }

    void CreateController()
    {
        Vector3 spawnPoint = GetPosSpawn();
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), spawnPoint, Quaternion.identity);
    }
    

    public static Vector3 GetPosSpawn()
    {
        GameObject spawn = GameObject.FindWithTag("spawn");
        Vector3 pos = spawn.transform.position;
        spawn.SetActive(false);
        return pos;
    }

}