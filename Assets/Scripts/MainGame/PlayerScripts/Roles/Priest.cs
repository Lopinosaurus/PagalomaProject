using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Priest : Villager
    {
        public List<Role> _targets = new List<Role>();
        public Role lastPlayerShielded = null;

        public void UpdateTarget(Collider other, bool add) // Add == true -> add target to targets list, otherwise remove target from targets
        {
            if (other.CompareTag("Player"))
            {
                Role tempTarget = null;
                foreach (Role role in other.GetComponents<Role>())
                {
                    if (role.enabled) tempTarget = role;
                }
                
                if (tempTarget != null && !(tempTarget is Priest))
                {
                    if (add)
                    {
                        _targets.Add(tempTarget);
                        Debug.Log("[+] Priest target added: "+tempTarget.name);
                    } else if (_targets.Contains(tempTarget))
                    {
                        _targets.Remove(tempTarget);
                        Debug.Log("[-] Priest target removed: "+tempTarget.name);
                    }
                }
            }
            UpdateActionText();
        }
        
        public override void UpdateActionText()
        {
            if (_photonView.IsMine)
            {
                if (_targets.Count > 0 && hasCooldown == false) actionText.text = "Press E to Give Shield";
                else actionText.text = "";
            }
        }

         public override void UseAbility()
        {
            GiveShield();
        }

        private void GiveShield()
        {
            Debug.Log("E pressed and you are a Priest, you gonna give a shield");
            if (!hasCooldown)
            {
                if (_targets.Count > 0)
                {
                    Role target = _targets[_targets.Count - 1];
                    if (target.isAlive == false)
                    {
                        Debug.Log("[-] GiveShield: Target is dead");
                        return;
                    }

                    if (lastPlayerShielded == target)
                    {
                        Debug.Log("[-] GiveShield: Can't give the same person a shield twice in a row");
                        RoomManager.Instance.UpdateInfoText($"You can not give the same person a shield twice in a row");
                        return;
                    }
                    hasCooldown = true;
                    target.hasShield = true;
                    lastPlayerShielded = target;
                    _targets.Remove(target);
                    UpdateActionText();
                    RoomManager.Instance.UpdateInfoText($"You gave a shield to {target.username}!");
                    _photonView.RPC("RPC_GiveShield", RpcTarget.Others, target.userId);
                    
                }
                else
                {
                    Debug.Log("[-] Can't Give Shield: No target");
                }
            }
            else
            {
                Debug.Log("[-] Can't Give Shield: You have a Cooldown");
            }
        }

        [PunRPC]
        public void RPC_GiveShield(string userId)
        {
            Role target = null;
            foreach (Role player in RoomManager.Instance.players) // Get target with corresponding userId
            {
                if (player.userId == userId) target = player;
            }

            if (target != null)
            {
                if (target.isAlive) target.hasShield = true;
                else Debug.Log($"[-] RPC_GiveShield({userId}): Can't give a shield, Target is dead");
            }
            else Debug.Log($"[-] RPC_GiveShield({userId}): Can't give a shield, target = null");
        }
    }
}
