using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    [Serializable]
    public class Werewolf : Role
    {
        [SerializeField] private GameObject VillagerRenderer;
        [SerializeField] private GameObject WereWolfRenderer;
        [SerializeField] private GameObject Particles;

        public List<Role> _targets = new List<Role>();
        public bool isTransformed;

        public float werewolfPowerDuration = 60;
        public float afterAttackCooldown = 5;
        
        public override void UpdateActionText()
        {
            if (_photonView.IsMine)
                if (actionText)
                {
                    if (isTransformed && _targets.Count > 0 && hasCooldown == false)
                        actionText.text = "Press E to Kill";
                    else if (VoteMenu.Instance.isNight && hasCooldown == false && isTransformed == false)
                        actionText.text = "Press E to Transform";
                    else actionText.text = "";
                }
        }
        
        /// <summary>
        ///   <para>Adds or removes the targets list.</para>
        /// </summary>
        /// <param name="other">The collider that will be updated.</param>
        /// <param name="add">When true, adds the collider to the targets list, otherwise will try to remove it.</param>
        public void UpdateTarget(Collider other, bool add)
        {
            if (isTransformed == false) return;
            if (other.CompareTag("Player"))
            {
                Role tempTarget = null;
                foreach (Role role in other.GetComponents<Role>()) if (role.enabled) tempTarget = role;

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
                else UpdateTransformation(true);
            }
        }

        public void UpdateTransformation(bool goingToWerewolf)
        {
            if (goingToWerewolf && !isTransformed)
            {
                if (_photonView.IsMine) _photonView.RPC(nameof(RPC_Transformation), RpcTarget.Others);
                StartCoroutine(WerewolfTransform(true));
            }
            else if (isTransformed)
            {
                if (_photonView.IsMine) _photonView.RPC(nameof(RPC_Detransformation), RpcTarget.Others);
                StartCoroutine(WerewolfTransform(false));
            }
        }
    
        private IEnumerator WerewolfTransform(bool goingToWerewolf)
        {
            // Particles to dissimulate werewolf transition
            GameObject particles = Instantiate(Particles, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            particles.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            particles.transform.parent = transform;

            // Deactivate controls
            if (_photonView.IsMine) _playerInput.SwitchCurrentActionMap("UI");

            // Transformation 
            if (goingToWerewolf)
            {
                isTransformed = true;
                _playerMovement.isWerewolfTransformedMult = true;
                if (_photonView.IsMine) StartCoroutine(DeTransformationCoroutine());
            }
            // DeTransformation
            else
            {
                // Removes speed boost
                isTransformed = false;
                _playerMovement.isWerewolfTransformedMult = false;
                SetCooldownInfinite();
            }

            UpdateActionText();

            // Wait
            yield return new WaitForSeconds(1);

            // TODO improve visual transition
            VillagerRenderer.SetActive(!goingToWerewolf);
            WereWolfRenderer.SetActive(goingToWerewolf);

            // Changes the animator
            _playerAnimation.EnableWerewolfAnimations(goingToWerewolf);

            // Wait for 4 seconds
            yield return new WaitForSeconds(4);

            // Reactivate controls
            if (_photonView.IsMine) _playerInput.SwitchCurrentActionMap("Player");
            Debug.Log(_playerInput.currentActionMap);
        }

        private IEnumerator DeTransformationCoroutine()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();

            // Will bring the werewolf back to human when the cooldown runs out
            SetPowerTimer(werewolfPowerDuration);
            while (hasCooldown) yield return waitForFixedUpdate;

            UpdateTransformation(false);
        }

        private void KillTarget() // TODO: Add kill animation
        {
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

                    // Waits a few seconds before re-enabling attack
                    SetCooldown(afterAttackCooldown);
                    // it also pauses the timer for the power
                    PausePowerTimer();
                    ResumePowerTimer(afterAttackCooldown);
                    // it also slows the Werewolf
                    _playerMovement.StartModifySpeed(afterAttackCooldown, 0.5f, 0.1f, 0.7f);
                    
                    UpdateActionText();
                    if (target.hasShield)
                    {
                        RoomManager.Instance.UpdateInfoText("Kill attempt failed because the player has a shield!");
                        return;
                    }

                    Debug.Log("E pressed and you are a Werewolf, you gonna kill someone");
                    
                    target.Die();
                    _targets.Remove(target);
                    _photonView.RPC(nameof(RPC_KillTarget), RpcTarget.Others, target.userId);

                    _playerAnimation.EnableWerewolfAttackAnimation();
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
        public void RPC_KillTarget(string _userId)
        {
            Role target = null;
            foreach (Role player in RoomManager.Instance.players) // Get target with corresponding userId
                if (player.userId == _userId)
                    target = player;

            if (target != null)
            {
                if (target.isAlive) target.Die();
                else Debug.Log($"[-] RPC_KillTarget({_userId}): Can't kill, Target is already dead");
            }
            else
            {
                Debug.Log($"[-] RPC_KillTarget({_userId}): Can't kill, target = null");
            }
        }

        [PunRPC]
        public void RPC_Transformation() => UpdateTransformation(true);

        [PunRPC]
        public void RPC_Detransformation() => UpdateTransformation(false);
    }
}