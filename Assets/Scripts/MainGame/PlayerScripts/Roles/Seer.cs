using System.Collections.Generic;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Seer : Villager
    {
        public List<Role> targets = new List<Role>();
        
        public readonly string FriendlyRoleName = "Seer";

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


            UpdateActionText();
        }

        public override void UpdateActionText(ATMessage message)
        {
            if (!PlayerController.photonView.IsMine) return;
            if (targets.Count > 0 && PlayerController.powerTimer.IsNotZero) ActionText.text = "Press E to Reveal Role";
            else ActionText.text = "";
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

            Role target = targets[targets.Count - 1];
            
            if (target.isAlive == false)
            {
                Debug.Log("[-] Can't reveal role: Target is dead");
                return;
            }

            Debug.Log($"[+] The Role of the target is: {target.roleName}");
            
            string displayedRole = target.roleName;
            if (target is Lycan lycan) displayedRole = lycan.FriendlyRoleName;
            
            RoomManager.Instance.UpdateInfoText($"You revealed a {displayedRole}");
            
            PlayerController.powerTimer.SetInfinite();
        }
    }
}