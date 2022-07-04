using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Priest : Villager
    {
        public List<Role> targets = new List<Role>();
        public Role lastPlayerShielded;

        public override void UpdateTarget(Collider other, bool add) // Add == true -> add target to targets list, otherwise remove target from targets
        {
            if (other is not CharacterController || !other.CompareTag("Player")) return;

            // Adds or removes role
            if (other.TryGetComponent(out Role targetRole))
            {
                if (add)
                {
                    targets.Add(targetRole);
                    Debug.Log("[+] Priest target added: " + targetRole.name);
                }
                else
                {
                    targets.Remove(targetRole);
                    Debug.Log("[-] Priest target removed: " + targetRole.name);
                }
            }


            UpdateActionText();
        }

        public override void UpdateActionText(ATMessage message)
        {
            if (!PlayerController.photonView.IsMine) return;
            
            if (targets.Count > 0 && ArePowerAndCooldownValid) ActionText.text = "Press E to Give Shield";
            else ActionText.text = "";
        }

        public override void UseAbility()
        {
            if (!CanUseAbilityGeneric()) return;
            
            GiveShield();
        }

        private void GiveShield()
        {
            if (targets.Count <= 0)
            {
                Debug.Log("[-] Can't Give Shield: No target");
                return;
            }

            Role target = targets[^1];
            
            if (target.isAlive == false)
            {
                Debug.Log("[-] GiveShield: Target is dead");
                return;
            }

            if (lastPlayerShielded == target)
            {
                Debug.Log("[-] GiveShield: Can't give the same person a shield twice in a row");
                RoomManager.Instance.UpdateInfoText("You can not give the same person a shield twice in a row");
                return;
            }

            // Makes it so that the power is only usable once (per night)
            PlayerController.powerTimer.SetInfinite();

            target.hasShield = true;
            lastPlayerShielded = target;

            UpdateActionText();
            RoomManager.Instance.UpdateInfoText($"You gave a shield to {target.username} !");
            PlayerController.photonView.RPC(nameof(RPC_GiveShield), RpcTarget.Others, target.userId);
        }

        [PunRPC]
        public void RPC_GiveShield(string userId)
        {
            Role target = RoomManager.Instance.players.FirstOrDefault(player => player.userId == userId);

            if (target != null)
            {
                if (target.isAlive) target.hasShield = true;
                else Debug.Log($"[-] RPC_GiveShield({userId}): Can't give a shield, Target is dead");
            }
            else
            {
                Debug.Log($"[-] RPC_GiveShield({userId}): Can't give a shield, target = null");
            }
        }
    }
}