using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Priest : Villager
    {
        public List<Role> _targets = new List<Role>();
        public Role lastPlayerShielded;

        public void UpdateTarget(Collider other, bool add) // Add == true -> add target to targets list, otherwise remove target from targets
        {
            if (other.CompareTag("Player"))
            {
                if (other.TryGetComponent(out Role tempTarget) && !(tempTarget is Priest))
                {
                    if (add)
                    {
                        _targets.Add(tempTarget);
                        Debug.Log("[+] Priest target added: " + tempTarget.name);
                    }
                    else if (_targets.Contains(tempTarget))
                    {
                        _targets.Remove(tempTarget);
                        Debug.Log("[-] Priest target removed: " + tempTarget.name);
                    }
                }
            }

            UpdateActionText();
        }

        public override void UpdateActionText()
        {
            if (!_photonView.IsMine) return;
            
            if (_targets.Count > 0 && arePowerAndCooldownValid) actionText.text = "Press E to Give Shield";
            else actionText.text = "";
        }

        public override void UseAbility()
        {
            if (!CanUseAbilityGeneric()) return;
            
            GiveShield();
        }

        private void GiveShield()
        {
            if (_targets.Count <= 0)
            {
                Debug.Log("[-] Can't Give Shield: No target");
                return;
            }

            Role target = _targets[_targets.Count - 1];
            
            if (target.isAlive == false)
            {
                Debug.Log("[-] GiveShield: Target is dead");
                return;
            }

            if (lastPlayerShielded == target)
            {
                Debug.Log("[-] GiveShield: Can't give the same person a shield twice in a row");
                RoomManager.Instance.UpdateInfoText("You can not give the same person a shield twice in a row");
                return;
            }

            // Makes it so that the power is only usable once (per night)
            powerTimer.SetInfinite();

            target.hasShield = true;
            lastPlayerShielded = target;

            UpdateActionText();
            RoomManager.Instance.UpdateInfoText($"You gave a shield to {target.username} !");
            _photonView.RPC(nameof(RPC_GiveShield), RpcTarget.Others, target.userId);
        }

        [PunRPC]
        public void RPC_GiveShield(string _userId)
        {
            Role target = RoomManager.Instance.players.FirstOrDefault(player => player.userId == _userId);

            if (target != null)
            {
                if (target.isAlive) target.hasShield = true;
                else Debug.Log($"[-] RPC_GiveShield({_userId}): Can't give a shield, Target is dead");
            }
            else
            {
                Debug.Log($"[-] RPC_GiveShield({_userId}): Can't give a shield, target = null");
            }
        }
    }
}