using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class PlayerGenerator : MonoBehaviour, IPunInstantiateMagicCallback
    {
        private Villager Villager;
        private Seer Seer;
        private Werewolf Werewolf;
        private Lycan Lycan;
        private Spy Spy;
        private Role[] roles = new Role[3];
        [SerializeField] private GameObject _attackCollider;
        [SerializeField] private PhotonView _photonView;

        void Awake()
        {
            Villager = GetComponent<Villager>();
            Seer = GetComponent<Seer>();
            Werewolf = GetComponent<Werewolf>();
            Lycan = GetComponent<Lycan>();
            Spy = GetComponent<Spy>();
            roles = new[] { (Role)Villager, Seer, Werewolf, Lycan, Spy };
            _photonView = GetComponent<PhotonView>();
        }
        void Start()
        {
            _attackCollider.SetActive(true);
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] instanciationData = info.photonView.InstantiationData;
            string roleName = (string)instanciationData[0];
            Color color = RoomManager.Instance.colorsDict[(string)instanciationData[1]];
            string username = (string)instanciationData[2];
            string userId = (string)instanciationData[3];

            Role playerRole = null;
            
            if (roleName == "Villager")
            {
                playerRole = Villager;
                Destroy(_attackCollider);
            }
            
            if (roleName == "Lycan")
            {
                playerRole = Lycan;
                Destroy(_attackCollider);
            }
            
            if (roleName == "Spy")
            {
                playerRole = Spy;
                Destroy(_attackCollider);
            }
            
            if (roleName == "Seer")
            {
                playerRole = Seer;
            }
            
            if (roleName == "Werewolf")
            {
                playerRole = Werewolf;
            }
            
            if (playerRole != null)
            {
                // Disable other roles
                foreach (Role role in roles)
                    if (playerRole != role)
                        role.enabled = false;
                
                playerRole.Activate();
                playerRole.userId = userId;
                playerRole.username = username;
                playerRole.SetPlayerColor(color);
                
                // Add instantiated role to players list
                RoomManager.Instance.players.Add(playerRole);
                // Store reference to the local player
                if (info.Sender.IsLocal) RoomManager.Instance.localPlayer = playerRole;
                // Update Voting List
                VoteMenu.Instance.Add(playerRole);
            }
            else
            {
                Debug.LogError("[-] PlayerGenerator: playerRole is null, it should never happen");
            }

            // Deactivate _attackCollider for non-local players
            if (!info.Sender.IsLocal && _attackCollider != null)
            {
                Destroy(_attackCollider);
            }
        }
    }
}
