using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Werewolf : Role
    {
        // private bool _hasCooldown = false;
        public List<Role> _targets = new List<Role>();

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
                        Debug.Log("[+] Target added: "+tempTarget.name);
                    } else if (_targets.Contains(tempTarget))
                    {
                        _targets.Remove(tempTarget);
                        Debug.Log("[-] Target removed: "+tempTarget.name);
                    }
                }
            }
        }

        public override void KillTarget()
        {
            
            Debug.Log("E pressed and you are a Werewolf, you gonna kill someone");
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
                    _photonView.RPC("RPC_KillTarget", RpcTarget.Others, target.userId);
                    
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

        [PunRPC]
        public void RPC_KillTarget(int userId)
        {
            // foreach (Player player in PhotonNetwork.PlayerList) 
            // {
            //     print(player.UserId);
            // }
            // if (target.isAlive == false) target.Die();
        }
    }
}

