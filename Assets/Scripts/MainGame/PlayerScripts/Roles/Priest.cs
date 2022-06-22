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

        public void UpdateTarget(Collider other, bool add) // Add == true -> add target to targets list, otherwise remove target from targets
        {
            if (other.CompareTag("Player"))
            {
                if (other.TryGetComponent(out Role tempTarget) && !(tempTarget is Priest))
                {
                    if (add)
                    {
                        targets.Add(tempTarget);
                        Debug.Log("[+] Priest target added: " + tempTarget.name);
                    }
                    else if (targets.Contains(tempTarget))
                    {
                        targets.Remove(tempTarget);
                        Debug.Log("[-] Priest target removed: " + tempTarget.name);
                    }
                }
            }

            UpdateActionText();
        }

        public override void UpdateActionText()
        {
            if (!PhotonView.IsMine) return;
            
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

            Role target = targets[targets.Count - 1];
            
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
            PowerTimer.SetInfinite();

            target.hasShield = true;
            lastPlayerShielded = target;

            UpdateActionText();
            RoomManager.Instance.UpdateInfoText($"You gave a shield to {target.username} !");
            PhotonView.RPC(nameof(RPC_GiveShield), RpcTarget.Others, target.userId);
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