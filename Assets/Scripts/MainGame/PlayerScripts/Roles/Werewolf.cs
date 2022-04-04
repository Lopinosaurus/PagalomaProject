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
            UpdateActionText();
        }

        public override void KillTarget() // TODO: Add kill animation
        {
            Debug.Log("E pressed and you are a Werewolf, you gonna kill someone");
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
                    // _hasCooldown = true;
                    target.Die();
                    _targets.Remove(target);
                    UpdateActionText();
                    _photonView.RPC("RPC_KillTarget", RpcTarget.Others, target.userId);
                    
                    
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

        private void UpdateActionText()
        {
            if (_photonView.IsMine)
            {
                if (_targets.Count > 0 && _hasCooldown == false) actionText.text = "Press E to Kill";
                else actionText.text = "";
            }
        }

        [PunRPC]
        public void RPC_KillTarget(string userId) // TODO: Add kill animation
        {
            Role target = null;
            foreach (Role player in RoomManager.Instance.players) // Get target with corresponding userId
            {
                if (player.userId == userId) target = player;
            }

            if (target != null)
            {
                if (target.isAlive) target.Die();
                else Debug.Log($"[-] RPC_KillTarget({userId}): Can't kill, Target is already dead");
            }
            else Debug.Log($"[-] RPC_KillTarget({userId}): Can't kill, target = null");
        }
    }
}

