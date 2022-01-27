using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class SpawnPlayers : MonoBehaviour
{
    // public GameObject playerPrefab;
    private void Start()
    {
        // Vector3 spawnPosition = new Vector3(0, 0, 0);
        // PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), Vector3.zero, Quaternion.identity);
    }
}