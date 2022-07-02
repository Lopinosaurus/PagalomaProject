using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class PlayerGenerator : MonoBehaviour, IPunInstantiateMagicCallback
    {
        [SerializeField] private SphereCollider attackCollider;

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
                    playerRole = gameObject.AddComponent<Villager>();
                    Destroy(attackCollider);
                    break;
                case "Lycan":
                    playerRole = gameObject.AddComponent<Lycan>();
                    Destroy(attackCollider);
                    break;
                case "Spy":
                    playerRole = gameObject.AddComponent<Spy>();
                    Destroy(attackCollider);
                    break;
                case "Seer":
                    playerRole = gameObject.AddComponent<Seer>();
                    break;
                case "Werewolf":
                    playerRole = gameObject.AddComponent<Werewolf>();
                    break;
                case "Priest":
                    playerRole = gameObject.AddComponent<Priest>();
                    break;
            }

            playerRole!.userId = userId;
            playerRole.username = username;
            playerRole.SetPlayerColor(color);

            // Add instantiated role to players list
            RoomManager.Instance.players.Add(playerRole);

            // Store reference to the local player
            if (info.Sender.IsLocal)
            {
                RoomManager.Instance.localPlayer = playerRole;
                RoomManager.Instance.localPlayer.GetComponent<PlayerController>().SetPcRole(playerRole);
                Debug.Log($"New role set in PlayerController : {playerRole}");
            }

            // Update Voting List
            VoteMenu.Instance.Add(playerRole);

            // Destroys _attackCollider for non-local players
            if (!info.Sender.IsLocal) Destroy(attackCollider);
        }
    }
}