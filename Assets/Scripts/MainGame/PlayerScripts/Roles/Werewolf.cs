using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public readonly string friendlyRoleName = "Werewolf";
        
        public List<Role> _targets = new List<Role>();
        public bool isTransformed;

        public float werewolfPowerDuration = 60;
        public float afterAttackCooldown = 5;
        
        private const float earlyTransformationTransitionDuration = 1;
        private const float lateTransformationTransitionDuration = 4;

        private float totalTransformationTransitionDuration =>
            earlyTransformationTransitionDuration + lateTransformationTransitionDuration;

        public override void UpdateActionText()
        {
            if (_photonView.IsMine)
                if (actionText)
                {
                    if (isTransformed && _targets.Count > 0 && powerTimer.isNotZero && powerCooldown.isZero)
                        actionText.text = "Press E to Kill";
                    else if (VoteMenu.Instance.isNight && isTransformed == false)
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
                foreach (Role role in other.GetComponents<Role>())
                    if (role.enabled)
                        tempTarget = role;

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
            if (!CanUseAbilityGeneric()) return;
            
            if (isTransformed) KillTarget();
            else UpdateTransformation(true);
        }

        [ContextMenu(nameof(UpdateTransformation))]
        public void UpdateTransformation(bool goingToWerewolf)
        {
            // Won't transform into given state if already in this given state
            if (goingToWerewolf == isTransformed) return;

            if (!arePowerAndCooldownValid) return;

            if (_photonView.IsMine) _photonView.RPC(goingToWerewolf ? nameof(RPC_Transformation) : nameof(RPC_Detransformation), RpcTarget.Others);

            StartCoroutine(WerewolfTransform(goingToWerewolf));
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
            isTransformed = goingToWerewolf;
            _playerMovement.isWerewolfTransformedMult = goingToWerewolf;

            // Starts countdown
            if (goingToWerewolf)
            {
                powerTimer.Set(werewolfPowerDuration);
                powerTimer.Pause();
                powerTimer.Resume(totalTransformationTransitionDuration);
            }
            else powerTimer.Reset();

            if (goingToWerewolf && _photonView.IsMine) StartCoroutine(DeTransformationCoroutine());

            // Updates the messages on screen
            UpdateActionText();

            // Wait
            yield return new WaitForSeconds(earlyTransformationTransitionDuration);

            // TODO improve visual transition
            VillagerRenderer.SetActive(!goingToWerewolf);
            WereWolfRenderer.SetActive(goingToWerewolf);

            // Changes the animator
            _playerAnimation.EnableWerewolfAnimations(goingToWerewolf);

            // Wait for 4 seconds
            yield return new WaitForSeconds(lateTransformationTransitionDuration);

            // Reactivate controls
            if (_photonView.IsMine) _playerInput.SwitchCurrentActionMap("Player");
        }

        private IEnumerator DeTransformationCoroutine()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();

            // Will bring the werewolf back to human when the power timer runs out
            while (powerTimer.isNotZero) yield return waitForFixedUpdate;

            UpdateTransformation(false);
        }

        private void KillTarget()
        {
        // Cannot attack if no target
            if (_targets.Count == 0)
            {
                Debug.Log("[-] Can't kill: No target to kill");
                return;
            }

            Role target = _targets[_targets.Count - 1];

            // Cannot attack if target is dead
            if (target.isAlive == false)
            {
                Debug.Log("[-] Can't kill: Target is already dead");
                return;
            }

            // Cannot attack if target has shield
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

            // Waits a few seconds before re-enabling attack
            powerCooldown.Set(afterAttackCooldown);
            // it also pauses the timer for the power
            powerTimer.Pause();
            powerTimer.Resume(afterAttackCooldown);
            // it also slows the Werewolf
            _playerMovement.StartModifySpeed(afterAttackCooldown, 0.5f, 0.1f, 0.7f);

            UpdateActionText();

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


        [PunRPC]
        public void RPC_KillTarget(string _userId)
        {
            // Tries to get the matching player, and can be null if not found
            Role target = RoomManager.Instance.players.FirstOrDefault(player => player.userId == _userId);

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
        public void RPC_Transformation() => StartCoroutine(WerewolfTransform(true));
        [PunRPC]
        public void RPC_Detransformation() => StartCoroutine(WerewolfTransform(false));
    }
}