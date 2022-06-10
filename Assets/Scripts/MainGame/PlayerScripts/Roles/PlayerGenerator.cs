using System;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using MainGame.Menus;
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
        private Priest Priest;
        private Role[] roles;
        [SerializeField] private GameObject _attackCollider;

        private void Awake()
        {
            Villager = GetComponent<Villager>();
            Seer = GetComponent<Seer>();
            Werewolf = GetComponent<Werewolf>();
            Lycan = GetComponent<Lycan>();
            Spy = GetComponent<Spy>();
            Priest = GetComponent<Priest>();
            roles = new[] { (Role)Villager, Seer, Werewolf, Lycan, Spy, Priest };
            GetComponent<PhotonView>();
        }

        private void Start()
        {
            _attackCollider.SetActive(true);
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] instantiationData = info.photonView.InstantiationData;
            string roleName = (string)instantiationData[0];
            Color color = RoomManager.Instance.colorsDict[(string)instantiationData[1]];
            string username = (string)instantiationData[2];
            string userId = (string)instantiationData[3];

            Role playerRole = null;
            
            switch (roleName)
            {
                case "Villager":
                    playerRole = Villager;
                    Destroy(_attackCollider);
                    break;
                case "Lycan":
                    playerRole = Lycan;
                    Destroy(_attackCollider);
                    break;
                case "Spy":
                    playerRole = Spy;
                    Destroy(_attackCollider);
                    break;
                case "Seer":
                    playerRole = Seer;
                    break;
                case "Werewolf":
                    playerRole = Werewolf;
                    break;
                case "Priest":
                    playerRole = Priest;
                    break;
            }

            if ((bool)playerRole) // checks if null with that
            {
                // Disable other roles
                foreach (Role role in roles)
                    if (playerRole != role)
                        role.enabled = false;
                
                playerRole!.Activate();
                playerRole.userId = userId;
                playerRole.username = username;
                playerRole.SetPlayerColor(color);
                
                // Add instantiated role to players list
                RoomManager.Instance.players.Add(playerRole);

                // Store reference to the local player
                if (info.Sender.IsLocal)
                {
                    RoomManager.Instance.localPlayer = playerRole;
                    RoomManager.Instance.localPlayer.GetComponent<PlayerController>()._role = playerRole;
                    Debug.Log($"New role set in PlayerController : {playerRole}");
                }
                
                // Update Voting List
                VoteMenu.Instance.Add(playerRole);
            }
            else
            {
                Debug.LogError("[-] PlayerGenerator: playerRole is null, it should never happen");
            }

            // Deactivate _attackCollider for non-local players
            if (!info.Sender.IsLocal && _attackCollider)
            {
                Destroy(_attackCollider);
            }
        }
    }
}
