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
            if (other.CompareTag("Player"))
            {
                Role tempTarget = null;
                foreach (Role role in other.GetComponents<Role>())
                    if (role.enabled)
                        tempTarget = role;

                if (tempTarget != null && !(tempTarget is Seer))
                {
                    if (add)
                    {
                        targets.Add(tempTarget);
                        Debug.Log("[+] Seer target added: " + tempTarget.name);
                    }
                    else if (targets.Contains(tempTarget))
                    {
                        targets.Remove(tempTarget);
                        Debug.Log("[-] Seer target removed: " + tempTarget.name);
                    }
                }
            }

            UpdateActionText();
        }

        public override void UpdateActionText()
        {
            if (PhotonView.IsMine)
            {
                if (targets.Count > 0 && PowerTimer.IsNotZero) ActionText.text = "Press E to Reveal Role";
                else ActionText.text = "";
            }
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
            
            PowerTimer.SetInfinite();
        }
    }
}