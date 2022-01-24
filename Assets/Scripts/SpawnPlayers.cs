using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Random = System.Random;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    private void Start()
    {
        Vector3 spawnPosition = new Vector3(0, 0, 0);
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
    }
}
