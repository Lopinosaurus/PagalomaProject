using System.Collections.Generic;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Seer : Villager
    {
        public static readonly string FriendlyRoleName = "Seer";

        protected override Role.AtMessage GetAtMessage()
        {
            if (ArePowerAndCooldownValid)
            {
                if (targets.Count > 0) return Role.AtMessage.PowerReadyToUse;
                return Role.AtMessage.Clear;
            }
            return Role.AtMessage.PowerOnCooldown;
        }

        private new void Awake()
        {
            base.Awake();
            
            AtMessageDict = new()
            {
                {Role.AtMessage.PowerReadyToUse, $"{RebindSystem.mainActionInputName}: Reveal role"},
                {Role.AtMessage.PowerOnCooldown, "Can't reveal any role until next night"},
                {Role.AtMessage.Clear, ""}
            };
        }
        
        public List<Role> targets = new List<Role>();
        
        public override void UpdateTarget(Collider other, bool add) // Add == true -> add target to targets list, otherwise remove target from targets
        {
            if (other is not CharacterController || !other.CompareTag("Player")) return;

            // Adds or removes role
            if (other.TryGetComponent(out Role targetRole))
            {
                if (add)
                {
                    targets.Add(targetRole);
                    Debug.Log("[+] Seer target added: " + targetRole.name);
                }
                else
                {
                    targets.Remove(targetRole);
                    Debug.Log("[-] Seer target removed: " + targetRole.name);
                }
            }
            
            UpdateActionText(GetAtMessage());
        }

        public override void UseAbility()
        {
            if (!CanUseAbilityGeneric()) return;
            
            RevealRole();
        }

        private void RevealRole()
        {
            if (0 == targets.Count)
            {
                Debug.Log("[-] Can't reveal role: No target");
                return;
            }

            Role target = targets[^1];
            
            if (target.isAlive == false)
            {
                Debug.Log("[-] Can't reveal role: Target is dead");
                return;
            }

            Debug.Log($"[+] The Role of the target is: {target.roleName}");
            
            string displayedRole = target.roleName;
            if (target is Lycan) displayedRole = Lycan.FriendlyRoleName;
            
            RoomManager.Instance.UpdateInfoText($"You revealed a {displayedRole}");
            
            PlayerController.powerCooldown.SetInfinite();
            UpdateActionText(Role.AtMessage.PowerOnCooldown);
        }
    }
}