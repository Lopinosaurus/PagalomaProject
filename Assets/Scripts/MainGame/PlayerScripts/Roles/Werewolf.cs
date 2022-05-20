using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.Menus;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MainGame.PlayerScripts.Roles
{
    public class Werewolf : Role
    {
        [SerializeField] private GameObject VillagerRenderer;
        [SerializeField] private GameObject WereWolfRenderer;
        [SerializeField] private GameObject Particles;
        [SerializeField] private PlayerInput playerInput;
        
        public List<Role> _targets = new List<Role>();
        public bool isTransformed = false;

        public override void UpdateActionText()
        {
            if (_photonView.IsMine)
            {
                if (isTransformed && _targets.Count > 0 && hasCooldown == false)
                { 
                    actionText.text = "Press E to Kill";
                }
                else if (VoteMenu.Instance.isNight && hasCooldown == false)
                {
                    actionText.text = "Press E to Transform";
                }
                else actionText.text = "";
            }
        }

        public void
            UpdateTarget(Collider other,
                bool add) // Add == true -> add target to targets list, otherwise remove target from targets
        {
            if (isTransformed == false) return;
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
                        Debug.Log("[+] Werewolf target added: " + tempTarget.name);
                    }
                    else if (_targets.Contains(tempTarget))
                    {
                        _targets.Remove(tempTarget);
                        Debug.Log("[-] Werewolf target removed: " + tempTarget.name);
                    }
                }
            }

            UpdateActionText();
        }

        public override void UseAbility()
        {
            if (!hasCooldown && VoteMenu.Instance.isNight)
            {
                if (isTransformed) KillTarget();
                else Transformation();
            }
        }

        private void Transformation()
        {
            if (_photonView.IsMine) _photonView.RPC("RPC_Transformation", RpcTarget.Others);
            StartCoroutine(WerewolfTransform(true));
        }

        private IEnumerator WerewolfTransform(bool isTransformation)
        {
            GameObject p = Instantiate(Particles, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            p.transform.rotation = Quaternion.Euler(new float3(-90, 0, 0));
            playerInput.SwitchCurrentActionMap("UI"); // Deactivate controls
            yield return new WaitForSeconds(1);
            VillagerRenderer.SetActive(!isTransformation);
            WereWolfRenderer.SetActive(isTransformation);
            yield return new WaitForSeconds(4);

            // Changes the animator
            _playerAnimation.EnableWerewolfAnimations(isTransformation);
            
            if (isTransformation) // Transformation 
            {
                isTransformed = true;
                _playerMovement.StartModifySpeed(60, 1.7f, 0, 1);
                if (_photonView.IsMine) StartCoroutine(DeTransformationCoroutine(60));
            }
            else // DeTransformation
            {
                isTransformed = false;
                hasCooldown = true;
            }

            UpdateActionText();
            playerInput.SwitchCurrentActionMap("Player"); // Reactivate controls
        }

        private IEnumerator DeTransformationCoroutine(int delay)
        {
            yield return new WaitForSeconds(delay);
            DeTransformation();
        }

        public void DeTransformation()
        {
            if (isTransformed)
            {
                if (_photonView.IsMine) _photonView.RPC("RPC_DeTransformation", RpcTarget.Others);
                StartCoroutine(WerewolfTransform(false));
            }
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

                    //
                    // PLAY KILL ANIMATION HERE
                    //
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

        [PunRPC]
        public void RPC_Transformation()
        {
            Transformation();
        }

        [PunRPC]
        public void RPC_DeTransformation()
        {
            DeTransformation();
        }
    }
}