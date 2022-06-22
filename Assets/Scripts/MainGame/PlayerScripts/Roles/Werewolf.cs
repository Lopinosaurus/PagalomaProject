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
        [SerializeField] private GameObject villagerRenderer;
        [SerializeField] private GameObject wereWolfRenderer;
        [SerializeField] private GameObject particles;

        public readonly string FriendlyRoleName = "Werewolf";
        
        public List<Role> targets = new List<Role>();
        public bool isTransformed;

        public float werewolfPowerDuration = 60;
        public float afterAttackCooldown = 5;
        
        private const float EarlyTransformationTransitionDuration = 1;
        private const float LateTransformationTransitionDuration = 4;

        private float TotalTransformationTransitionDuration =>
            EarlyTransformationTransitionDuration + LateTransformationTransitionDuration;

        public override void UpdateActionText()
        {
            if (PhotonView.IsMine)
                if (ActionText)
                {
                    if (isTransformed && targets.Count > 0 && powerTimer.IsNotZero && powerCooldown.IsZero)
                        ActionText.text = "Press E to Kill";
                    else if (VoteMenu.Instance.IsNight && isTransformed == false)
                        ActionText.text = "Press E to Transform";
                    else ActionText.text = "";
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
                        targets.Add(tempTarget);
                        Debug.Log("[+] Werewolf target added: " + tempTarget.name);
                    }
                    else if (targets.Contains(tempTarget))
                    {
                        targets.Remove(tempTarget);
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

            if (!ArePowerAndCooldownValid) return;

            if (PhotonView.IsMine) PhotonView.RPC(goingToWerewolf ? nameof(RPC_Transformation) : nameof(RPC_Detransformation), RpcTarget.Others);

            StartCoroutine(WerewolfTransform(goingToWerewolf));
        }

        private IEnumerator WerewolfTransform(bool goingToWerewolf)
        {
            // Particles to dissimulate werewolf transition
            GameObject particles = Instantiate(this.particles, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            particles.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            particles.transform.parent = transform;

            // Deactivate controls
            if (PhotonView.IsMine) playerInput.SwitchCurrentActionMap("UI");

            // Transformation
            isTransformed = goingToWerewolf;
            PlayerMovement.isWerewolfTransformedMult = goingToWerewolf;

            // Starts countdown
            if (goingToWerewolf)
            {
                powerTimer.Set(werewolfPowerDuration);
                powerTimer.Pause();
                powerTimer.Resume(TotalTransformationTransitionDuration);
            }
            else powerTimer.Reset();

            if (goingToWerewolf && PhotonView.IsMine) StartCoroutine(DeTransformationCoroutine());

            // Updates the messages on screen
            UpdateActionText();

            // Wait
            yield return new WaitForSeconds(EarlyTransformationTransitionDuration);

            // TODO improve visual transition
            villagerRenderer.SetActive(!goingToWerewolf);
            wereWolfRenderer.SetActive(goingToWerewolf);

            // Changes the animator
            PlayerAnimation.EnableWerewolfAnimations(goingToWerewolf);

            // Wait for 4 seconds
            yield return new WaitForSeconds(LateTransformationTransitionDuration);

            // Reactivate controls
            if (PhotonView.IsMine) playerInput.SwitchCurrentActionMap("Player");
        }

        private IEnumerator DeTransformationCoroutine()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();

            // Will bring the werewolf back to human when the power timer runs out
            while (powerTimer.IsNotZero) yield return waitForFixedUpdate;

            UpdateTransformation(false);
        }

        private void KillTarget()
        {
        // Cannot attack if no target
            if (targets.Count == 0)
            {
                Debug.Log("[-] Can't kill: No target to kill");
                return;
            }

            Role target = targets[targets.Count - 1];

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
            targets.Remove(target);
            PhotonView.RPC(nameof(RPC_KillTarget), RpcTarget.Others, target.userId);

            PlayerAnimation.EnableWerewolfAttackAnimation();

            // Waits a few seconds before re-enabling attack
            powerCooldown.Set(afterAttackCooldown);
            // it also pauses the timer for the power
            powerTimer.Pause();
            powerTimer.Resume(afterAttackCooldown);
            // it also slows the Werewolf
            PlayerMovement.StartModifySpeed(afterAttackCooldown, 0.5f, 0.1f, 0.7f);

            UpdateActionText();

            UpdateActionText();
            if (target.hasShield)
            {
                RoomManager.Instance.UpdateInfoText("Kill attempt failed because the player has a shield!");
                return;
            }

            Debug.Log("E pressed and you are a Werewolf, you gonna kill someone");

            target.Die();
            targets.Remove(target);
            PhotonView.RPC(nameof(RPC_KillTarget), RpcTarget.Others, target.userId);

            PlayerAnimation.EnableWerewolfAttackAnimation();
        }


        [PunRPC]
        public void RPC_KillTarget(string userId)
        {
            // Tries to get the matching player, and can be null if not found
            Role target = RoomManager.Instance.players.FirstOrDefault(player => player.userId == userId);

            if (target != null)
            {
                if (target.isAlive) target.Die();
                else Debug.Log($"[-] RPC_KillTarget({userId}): Can't kill, Target is already dead");
            }
            else
            {
                Debug.Log($"[-] RPC_KillTarget({userId}): Can't kill, target = null");
            }
        }

        [PunRPC]
        public void RPC_Transformation() => StartCoroutine(WerewolfTransform(true));
        [PunRPC]
        public void RPC_Detransformation() => StartCoroutine(WerewolfTransform(false));
    }
}