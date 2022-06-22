using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class PlayerGenerator : MonoBehaviour, IPunInstantiateMagicCallback
    {
        [SerializeField] private GameObject attackCollider;
        private Lycan _lycan;
        private Priest _priest;
        private Role[] _roles;
        private Seer _seer;
        private Spy _spy;
        private Villager _villager;
        private Werewolf _werewolf;

        private void Awake()
        {
            _villager = GetComponent<Villager>();
            _seer = GetComponent<Seer>();
            _werewolf = GetComponent<Werewolf>();
            _lycan = GetComponent<Lycan>();
            _spy = GetComponent<Spy>();
            _priest = GetComponent<Priest>();
            _roles = new[] {(Role) _villager, _seer, _werewolf, _lycan, _spy, _priest};
            GetComponent<PhotonView>();
        }

        private void Start()
        {
            attackCollider.SetActive(true);
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] instantiationData = info.photonView.InstantiationData;
            var roleName = (string) instantiationData[0];
            Color color = RoomManager.Instance.ColorsDict[(string) instantiationData[1]];
            var username = (string) instantiationData[2];
            var userId = (string) instantiationData[3];

            Role playerRole = null;

            switch (roleName)
            {
                case "Villager":
                    playerRole = _villager;
                    Destroy(attackCollider);
                    break;
                case "Lycan":
                    playerRole = _lycan;
                    Destroy(attackCollider);
                    break;
                case "Spy":
                    playerRole = _spy;
                    Destroy(attackCollider);
                    break;
                case "Seer":
                    playerRole = _seer;
                    break;
                case "Werewolf":
                    playerRole = _werewolf;
                    break;
                case "Priest":
                    playerRole = _priest;
                    break;
            }

            if ((bool) playerRole) // checks if null with that
            {
                // Disable other roles
                foreach (Role role in _roles)
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
                    RoomManager.Instance.localPlayer.GetComponent<PlayerController>().role = playerRole;
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
            if (!info.Sender.IsLocal && attackCollider) Destroy(attackCollider);
        }
    }
}