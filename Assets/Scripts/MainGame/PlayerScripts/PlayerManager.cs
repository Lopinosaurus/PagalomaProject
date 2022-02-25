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
        Vector3 spawnPoint = new Vector3(Random.Range (0, 10), 1, Random.Range (0, 10));
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), spawnPoint, Quaternion.identity);
    }
}