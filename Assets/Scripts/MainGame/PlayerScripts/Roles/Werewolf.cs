using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Werewolf : Role
    {
        // private bool _hasCooldown = false;
        private List<Role> _targets = new List<Role>();

        public void UpdateTarget(Collider other, bool add) // Add == true -> add target to targets list, otherwise remove target from targets
        {
            if (other.CompareTag("Player"))
            {
                Role tempTarget = (Role)other.GetComponent<Villager>();
                if (tempTarget != null)
                {
                    if (add)
                    {
                        _targets.Add(tempTarget);
                        // Debug.Log("[+] Target added: "+tempTarget.name);
                    } else if (_targets.Contains(tempTarget))
                    {
                        _targets.Remove(tempTarget);
                        // Debug.Log("[-] Target removed: "+tempTarget.name);
                    }
                }
            }
        }

        public override void KillTarget()
        {
            // Debug.Log("In KillTarget");
            //if (!_hasCooldown)
            //{
                if (_targets.Count > 0)
                {
                    Role target = _targets[_targets.Count - 1];
                    if (target.isAlive == false)
                    {
                        Debug.Log("[-] Can't kill: Target is already dead");
                        return;
                    }
                    transform.position = target.transform.position;
                    target.Die();
                    _targets.Remove(target);
                    //_hasCooldown = true;
                }
                else
                {
                    Debug.Log("[-] Can't kill: No target to kill");
                }
            //}
            //else
            //{
            //    Debug.Log("[-] Can't kill: You have a Cooldown");
            //}
        }
    }
}

