using System.Collections.Generic;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Seer : Villager
    {
        public List<Role> _targets = new List<Role>();
        
        public readonly string friendlyRoleName = "Seer";

        public void UpdateTarget(Collider other, bool add) // Add == true -> add target to targets list, otherwise remove target from targets
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
                        _targets.Add(tempTarget);
                        Debug.Log("[+] Seer target added: " + tempTarget.name);
                    }
                    else if (_targets.Contains(tempTarget))
                    {
                        _targets.Remove(tempTarget);
                        Debug.Log("[-] Seer target removed: " + tempTarget.name);
                    }
                }
            }

            UpdateActionText();
        }

        public override void UpdateActionText()
        {
            if (_photonView.IsMine)
            {
                if (_targets.Count > 0 && powerTimer.isNotZero) actionText.text = "Press E to Reveal Role";
                else actionText.text = "";
            }
        }

        public override void UseAbility()
        {
            if (!CanUseAbilityGeneric()) return;
            
            RevealRole();
        }

        private void RevealRole()
        {
            if (0 == _targets.Count)
            {
                Debug.Log("[-] Can't reveal role: No target");
                return;
            }

            Role target = _targets[_targets.Count - 1];
            
            if (target.isAlive == false)
            {
                Debug.Log("[-] Can't reveal role: Target is dead");
                return;
            }

            Debug.Log($"[+] The Role of the target is: {target.roleName}");
            
            string displayedRole = target.roleName;
            if (target is Lycan lycan) displayedRole = lycan.friendlyRoleName;
            
            RoomManager.Instance.UpdateInfoText($"You revealed a {displayedRole}");
            
            powerTimer.SetInfinite();
        }
    }
}