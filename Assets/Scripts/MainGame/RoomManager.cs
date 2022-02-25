using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon; // Used for OnRoomPropertiesUpdate
using UnityEngine.SceneManagement;
using System.IO;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    [SerializeField] private GameObject map; 
    private ExitGames.Client.Photon.Hashtable customGameProperties = new ExitGames.Client.Photon.Hashtable();

    private void Awake()
    {
        if (Instance) // Checks if another RoomManager exists
        {
            Destroy(gameObject); // There can only be one
            return;
        }
        DontDestroyOnLoad(gameObject); // I am the only one
        Instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (1 == scene.buildIndex) // MainGame scene
        {
            // Instantiate local player
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero,
                Quaternion.identity);
            
            // Generate Map Seed
            if (PhotonNetwork.IsMasterClient)
            {
                System.Random rng = new System.Random();
                int seed = rng.Next(0, 100000); // Generate random int between 0 and 100000
                customGameProperties["MapSeed"] = seed; // Add the seed to the Photon Hashtable
                PhotonNetwork.CurrentRoom.SetCustomProperties(customGameProperties); // Send the custom property to the server so that it is available for everyone in the room
            }
        }
    }

    // Callback when Custom Properties change
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        if (propertiesThatChanged.ContainsKey("MapSeed")) // MapSeed changed
        {
            int seed = (int) propertiesThatChanged["MapSeed"]; // Get the seed value
            Debug.Log("Game seed received: "+seed);
            map.GetComponent<Map>().Generate(seed); // Generate the map
        }
    }
}