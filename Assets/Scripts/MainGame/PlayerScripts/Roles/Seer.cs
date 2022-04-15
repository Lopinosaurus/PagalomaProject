using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Seer : Villager
    {
        public List<Role> _targets = new List<Role>();

        public void UpdateTarget(Collider other, bool add) // Add == true -> add target to targets list, otherwise remove target from targets
        {
            if (other.CompareTag("Player"))
            {
                Role tempTarget = null;
                foreach (Role role in other.GetComponents<Role>())
                {
                    if (role.enabled) tempTarget = role;
                }
                
                if (tempTarget != null && !(tempTarget is Seer))
                {
                    if (add)
                    {
                        _targets.Add(tempTarget);
                        Debug.Log("[+] Seer target added: "+tempTarget.name);
                    } else if (_targets.Contains(tempTarget))
                    {
                        _targets.Remove(tempTarget);
                        Debug.Log("[-] Seer target removed: "+tempTarget.name);
                    }
                }
            }
            UpdateActionText();
        }
        
         private void UpdateActionText()
        {
            if (_photonView.IsMine)
            {
                if (_targets.Count > 0 && _hasCooldown == false) actionText.text = "Press E to Reveal Role";
                else actionText.text = "";
            }
        }

         public override void UseAbility()
        {
            SpyTarget();
        }

        private void SpyTarget()
        {
            Debug.Log("E pressed and you are a Seer, you gonna get someone's role");
            //if (!_hasCooldown)
            //{
                if (_targets.Count > 0)
                {
                    Role target = _targets[_targets.Count - 1];
                    if (target.isAlive == false)
                    {
                        Debug.Log("[-] Can't spy: Target is dead");
                        return;
                    }
                    // _hasCooldown = true;
                    Debug.Log($"[+] The Role of the target is: {target.roleName}");
                    UpdateInfoText($"You revealed a {target.roleName}");
                }
                else
                {
                    Debug.Log("[-] Can't Spy: No target to spy");
                }
            //}
            //else
            //{
            //    Debug.Log("[-] Can't Spy: You have a Cooldown");
            //}
        }
    }
}
