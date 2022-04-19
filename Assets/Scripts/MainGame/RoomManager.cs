using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon; // Used for OnRoomPropertiesUpdate
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using MainGame;
using MainGame.PlayerScripts.Roles;
using TMPro;
using Random = System.Random;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    [SerializeField] private GameObject map;
    private ExitGames.Client.Photon.Hashtable customGameProperties = new ExitGames.Client.Photon.Hashtable();
    public string[] roles = new []{"Villager", "Werewolf", "Seer", "Villager", "Hunter", "Villager", "Werewolf", "Villager", "Villager", "Villager", "Villager", "Villager", "Werewolf"};

    public Dictionary<string, Color> colorsDict = new Dictionary<string, Color>()
    {
        { "red", Color.red },
        { "blue", Color.blue },
        { "yellow", Color.yellow },
        { "white", Color.white },
        { "black", Color.black },
        { "cyan", Color.cyan },
        { "magenta", Color.magenta },
        { "grey", Color.grey }
    };
    public string[] colors;
    
    public int nextPlayerRoleIndex;
    [SerializeField] private TMP_Text roleText;
    public TMP_Text actionText;
    public TMP_Text deathText;
    public TMP_Text infoText;
    public List<Role> players; // List of the Role of all the players
    public Role localPlayer; // Reference to the local player's role
    public List<Role> votes; 

    private void Awake()
    {
        if (Instance) // Checks if another RoomManager exists
        {
            Destroy(gameObject); // There can only be one
            return;
        }
        DontDestroyOnLoad(gameObject); // I am the only one
        Instance = this;
        IGMenuManager.Instance.loadingScreen.SetActive(true);
        players = new List<Role>();
        votes = new List<Role>();
        infoText.text = "";

        // Shuffle colors list
        Random rng = new Random();
        colors = new[] { "red", "blue", "yellow", "white", "black", "cyan", "magenta", "grey" };
        colors = colors.OrderBy(a => rng.Next()).ToArray();
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
        if (scene.buildIndex == 1) // MainGame scene
        {
            // Instantiate local PlayerManager
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
            
            // Master Client needs to generate the Map Seed
            if (PhotonNetwork.IsMasterClient)
            {
                GenerateMapSeed();
            }
        }
    }

    private void GenerateMapSeed()
    {
        System.Random rng = new System.Random();
        int seed = rng.Next(0, 100000); // Generate random int between 0 and 100000
        customGameProperties["MapSeed"] = seed; // Add the seed to the Photon Hashtable
        PhotonNetwork.CurrentRoom.SetCustomProperties(customGameProperties); // Send the custom property to the server so that it is available for everyone in the room
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
            IGMenuManager.Instance.loadingScreen.SetActive(false);
        }
    }

    public string GetNextRoleName()
    {
        return roles[nextPlayerRoleIndex];
    }
    
    public string GetNextColor()
    {
        return colors[nextPlayerRoleIndex];
    }

    public void DisplayRole(string roleName)
    {
        roleText.text = "You are "+roleName;
    }
    
    public void UpdateInfoText(string message = "")
    {
        StartCoroutine(UpdateInfoText(message, 5));
    }
       
    IEnumerator UpdateInfoText (string message, float delay) {
        infoText.text = message;
        yield return new WaitForSeconds(delay);
        infoText.text = "";
    }

    // Compute who has to be eliminated at the end of the vote
    public void ResolveVote() // Only MasterClient have access to this method
    {
        Dictionary<string, int> voteResults = new Dictionary<string, int>();
        foreach (Role vote in votes)
        {
            string userId = "";
            if (vote != null) userId = vote.userId;
            
            if (voteResults.ContainsKey(userId)) voteResults[userId]++;
            else voteResults.Add(userId, 1);
        }

        int max = 0;
        int max2 = 0;
        string votedUserId = "";
        foreach (string userId in voteResults.Keys)
        {
            if (voteResults[userId] > max)
            {
                max2 = max;
                max = voteResults[userId];
                votedUserId = userId;
            } else if (voteResults[userId] == max)
            {
                max2 = max;
            }
        }

        if (max == max2) votedUserId = "";
        VoteMenu.Instance.KillVotedPlayer(votedUserId);
    }
    
    public void ClearTargets() // Clear targets list of local player
    {
        if (localPlayer is Seer)
        {
            ((Seer)localPlayer)._targets = new List<Role>();
        }
        if (localPlayer is Werewolf)
        {
            ((Werewolf)localPlayer)._targets = new List<Role>();
        }
        localPlayer.UpdateActionText();
    }

    public void CheckIfEOG()
    {
        //TODO
    }
}
