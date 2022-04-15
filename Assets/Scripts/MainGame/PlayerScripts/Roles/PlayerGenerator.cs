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
        private Role[] roles = new Role[3];
        [SerializeField] private GameObject _attackCollider;
        [SerializeField] private PhotonView _photonView;

        void Awake()
        {
            Villager = GetComponent<Villager>();
            Seer = GetComponent<Seer>();
            Werewolf = GetComponent<Werewolf>();
            roles = new[] { (Role)Villager, Seer, Werewolf };
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
            string color = (string)instanciationData[1];
            string username = (string)instanciationData[2];
            string userId = (string)instanciationData[3];

            Role playerRole = null;
            
            if (roleName == "Villager")
            {
                playerRole = Villager;
                _attackCollider.SetActive(false);
            }
            
            if (roleName == "Seer")
            {
                playerRole = Seer;
            }
            
            if (roleName == "Werewolf")
            {
                playerRole = Werewolf;
            }

            Debug.Log($"playerRole = {playerRole}");
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
            }
            else
            {
                Debug.Log("[-] PlayerGenerator: playerRole is null, it should never happen");
            }

            // Try to deactivate _attackCollider for non-local players
            if (!info.Sender.IsLocal)
            {
                Debug.Log("Should deactivate _attackCollider of new remote Player clone");
                _attackCollider.SetActive(false); // This doesn't work, IDK why
            }
        }
    }
}
