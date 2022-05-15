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

                if (tempTarget != null && !(tempTarget is Werewolf))
                {
                    if (add)
                    {
                        _targets.Add(tempTarget);
                        Debug.Log("[+] Werewolf target added: "+tempTarget.name);
                    } else if (_targets.Contains(tempTarget))
                    {
                        _targets.Remove(tempTarget);
                        Debug.Log("[-] Werewolf target removed: "+tempTarget.name);
                    }
                }
            }
            UpdateActionText();
        }

        public override void UseAbility()
        {
            KillTarget();
        }
        private void KillTarget() // TODO: Add kill animation
        {
            Debug.Log("E pressed and you are a Werewolf, you gonna kill someone");
            if (!hasCooldown)
            {
                if (_targets.Count > 0)
                {
                    Role target = _targets[_targets.Count - 1];
                    if (target.isAlive == false)
                    {
                        Debug.Log("[-] Can't kill: Target is already dead");
                        return;
                    }
                    transform.position = target.transform.position;
                    hasCooldown = true;
                    UpdateActionText();
                    if (target.hasShield)
                    {
                        RoomManager.Instance.UpdateInfoText($"Kill attempt failed because the player has a shield!");
                        return;
                    }
                    target.Die();
                    _targets.Remove(target);
                    _photonView.RPC("RPC_KillTarget", RpcTarget.Others, target.userId);
                }
                else
                {
                    Debug.Log("[-] Can't kill: No target to kill");
                }
            }
            else
            {
                Debug.Log("[-] Can't kill: You have a Cooldown");
            }
        }

        public override void UpdateActionText()
        {
            if (_photonView.IsMine)
            {
                if (_targets.Count > 0 && hasCooldown == false) actionText.text = "Press E to Kill";
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

