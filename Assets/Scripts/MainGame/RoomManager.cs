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
using MainGame.Menus;
using MainGame.PlayerScripts.Roles;
using TMPro;
using Random = System.Random;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    public Map map;
    private ExitGames.Client.Photon.Hashtable customGameProperties = new ExitGames.Client.Photon.Hashtable();
    public Dictionary<string, Color> colorsDict = new Dictionary<string, Color>()
    {
        { "Red", Color.red },
        { "Blue", Color.blue },
        { "Yellow", Color.yellow },
        { "Lime", new Color(0.26f, 1f, 0f)},
        { "Pink", new Color(1f, 0f, 0.86f)},
        { "Cyan", Color.cyan },
        { "Orange", new Color(1f, 0.5f, 0f)},
        { "White", Color.white },
        { "Black", Color.black },
        { "Purple", new Color(0.71f, 0f, 1f) },
        { "Green", new Color(0f, 0.57f, 0.22f)},
        { "Grey", Color.grey },
        { "Brown", new Color(0.59f, 0.41f, 0.1f)},
        { "Teal", new Color(0f, 0.5f, 0.5f)},
        { "Maroon", new Color(0.5f, 0f, 0f)},
        { "Peach", new Color(0.95f, 0.82f, 0.74f)}
    };

    public string[] roles;
    public string[] colors;

    public int nextPlayerRoleIndex;
    [SerializeField] private TMP_Text roleText;
    public TMP_Text actionText;
    public TMP_Text deathText;
    public TMP_Text infoText;
    public Transform infoList;
    [SerializeField] private InfoListItem infoListItem;    
    
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
        //DontDestroyOnLoad(gameObject); // I am the only one
        Instance = this;
        IGMenuManager.Instance.loadingScreen.SetActive(true);
        players = new List<Role>();
        votes = new List<Role>();
        infoText.text = "";

        int numberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        roles = new []{"Werewolf","Werewolf","Werewolf", "Spy", "Seer", "Lycan", "Villager", "Priest", "Werewolf", "Villager", "Villager", "Werewolf", "Villager", "Villager", "Villager", "Villager", "Werewolf", "Villager"};
        colors = new []{ "Red", "Blue", "Yellow", "Lime", "Pink", "Cyan", "Orange", "White", "Black", "Purple", "Green", "Grey", "Brown", "Teal", "Maroon", "Peach" };
        colors = colors.Take(numberOfPlayers).ToArray();
        roles = roles.Take(numberOfPlayers).ToArray();
        foreach (string c in colors) Debug.Log(c);
        foreach (string c in roles) Debug.Log(c);
        // Shuffle colors and roles lists
        Random rng = new Random();
        colors = colors.OrderBy(a => rng.Next()).ToArray();
        roles = roles.OrderBy(a => rng.Next()).ToArray();
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
            map.Generate(seed); // Generate the map
            // Instantiate local PlayerManager
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
            // IGMenuManager.Instance.loadingScreen.SetActive(false);
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
        InfoListItem item = Instantiate(infoListItem, infoList);
        item.GetComponent<InfoListItem>().SetUp(message);
        Destroy(item.gameObject, 5);
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
            if (!VoteMenu.Instance.isNight) ((Werewolf)localPlayer).DeTransformation();
        }
        localPlayer.UpdateActionText();
    }

    public int CheckIfEOG() // Return 0 if not EOG | Return 1 if Werewolf won | Return 2 if Villager won | Return 3 if everyone is dead
    {
        int res = 0;
        bool isThereWerewolf = false;
        bool isThereVillager = false;
        
        foreach (Role role in players)
        {
            if (role.isAlive)
            {
                if (role is Werewolf) isThereWerewolf = true;
                else isThereVillager = true;
            }
        }

        if (isThereWerewolf && !isThereVillager) res = 1;
        else if (!isThereWerewolf && isThereVillager) res = 2;
        if (!isThereVillager && !isThereWerewolf) res = 3;
        return res;
    }

    public void DisplayEndScreen(int isEOG)
    {
        bool victory = isEOG != 3 && ((isEOG == 1) == localPlayer is Werewolf);
        IGMenuManager.Instance.OpenEndMenu(victory, isEOG);
    }
}
