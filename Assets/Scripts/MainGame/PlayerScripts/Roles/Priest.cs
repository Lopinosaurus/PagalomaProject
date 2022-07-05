using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Priest : Villager
    {
        private new void Awake()
        {
            base.Awake();
            
            AtMessageDict = new()
            {
                {AtMessage.PowerReadyToUse, $"{RebindSystem.mainActionInputName}: Give Shield"},
                {AtMessage.PowerOnCooldown, "Can't give a new shield until next night"},
                {AtMessage.Clear, ""}
            };
        }
        
        public List<Role> targets = new List<Role>();
        public Role lastPlayerShielded;

        protected override AtMessage GetAtMessage()
        {
            if (ArePowerAndCooldownValid)
            {
                if (targets.Count > 0) return AtMessage.PowerReadyToUse;
                return AtMessage.Clear;
            }
            return AtMessage.PowerOnCooldown;
        }

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
            
            UpdateActionText(GetAtMessage());
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

            target.isShielded = true;
            lastPlayerShielded = target;

            UpdateActionText(AtMessage.PowerOnCooldown);
            
            RoomManager.Instance.UpdateInfoText($"You gave a shield to {target.username} !");
            PlayerController.photonView.RPC(nameof(RPC_GiveShield), RpcTarget.Others, target.userId);
        }

        [PunRPC]
        public void RPC_GiveShield(string userId)
        {
            Role target = RoomManager.Instance.players.FirstOrDefault(player => player.userId == userId);

            if (target)
            {
                if (target.isAlive) target.isShielded = true;
                else Debug.Log($"[-] RPC_GiveShield({userId}): Can't give a shield, Target is dead");
            }
            else
            {
                Debug.Log($"[-] RPC_GiveShield({userId}): Can't give a shield, target = null");
            }
        }
    }
}