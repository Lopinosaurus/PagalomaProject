using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEditor;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    [Serializable]
    public class Werewolf : Role
    {
        private GameObject _villagerRender, _werewolfRender, _particles;

        public readonly string FriendlyRoleName = "Werewolf";
        
        public List<Role> targets = new List<Role>();
        public bool isTransformed;

        public float werewolfPowerDuration = 60;
        public float afterAttackCooldown = 5;
        
        private const float EarlyTransformationTransitionDuration = 1;
        private const float LateTransformationTransitionDuration = 4;

        private float TotalTransformationTransitionDuration =>
            EarlyTransformationTransitionDuration + LateTransformationTransitionDuration;

        public void SetupFields(GameObject villagerRender, GameObject werewolfRender, GameObject particles)
        {
            _villagerRender = villagerRender;
            _werewolfRender = werewolfRender;
            _particles = particles;
        }
        
        public override void UpdateActionText()
        {
            if (PhotonView.IsMine)
                if (ActionText)
                {
                    if (isTransformed && targets.Count > 0 && PowerTimer.IsNotZero && PowerCooldown.IsZero)
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
        public override void UpdateTarget(Collider other, bool add)
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

        public void UpdateTransformation(bool goingToWerewolf)
        {
            if (PhotonView.IsMine) PhotonView.RPC(goingToWerewolf ? nameof(RPC_Transformation) : nameof(RPC_Detransformation), RpcTarget.Others);

            StartCoroutine(WerewolfTransform(goingToWerewolf));
        }

        private IEnumerator WerewolfTransform(bool goingToWerewolf)
        {
            Debug.LogWarning($"{goingToWerewolf}");
            
            // Particles to dissimulate werewolf transition
            GameObject particles = Instantiate(this._particles, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            particles.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            particles.transform.parent = transform;

            // Deactivate controls
            if (PhotonView.IsMine) PlayerInput.SwitchCurrentActionMap("UI");

            // Transformation
            isTransformed = goingToWerewolf;
            PlayerMovement.isWerewolfTransformedMult = goingToWerewolf;

            // Starts countdown
            if (goingToWerewolf)
            {
                PowerTimer.Set(werewolfPowerDuration);
                PowerTimer.Pause();
                PowerTimer.Resume(TotalTransformationTransitionDuration);
            }
            else PowerTimer.SetInfinite();

            if (goingToWerewolf && PhotonView.IsMine) StartCoroutine(DeTransformationCoroutine());

            // Updates the messages on screen
            UpdateActionText();

            // Wait
            yield return new WaitForSeconds(EarlyTransformationTransitionDuration);

            // TODO improve visual transition
            _villagerRender.SetActive(!goingToWerewolf);
            _werewolfRender.SetActive(goingToWerewolf);

            // Changes the animator
            PlayerAnimation.EnableWerewolfAnimations(goingToWerewolf);

            // Wait for 4 seconds
            yield return new WaitForSeconds(LateTransformationTransitionDuration);

            // Reactivate controls
            if (PhotonView.IsMine) PlayerInput.SwitchCurrentActionMap("Player");
        }

        private IEnumerator DeTransformationCoroutine()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();

            // Will bring the werewolf back to human when the power timer runs out
            while (PowerTimer.IsNotZero)
            {
                Debug.Log($"Checking for Detransformation {PowerTimer.CountdownValue}");
                yield return waitForFixedUpdate;
            }

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
            PowerCooldown.Set(afterAttackCooldown);
            // it also pauses the timer for the power
            PowerTimer.Pause();
            PowerTimer.Resume(afterAttackCooldown);
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

        [PunRPC, ContextMenu("Transformation")]
        public void RPC_Transformation() => StartCoroutine(WerewolfTransform(true));
        [PunRPC, ContextMenu("Detransformation")]
        public void RPC_Detransformation() => StartCoroutine(WerewolfTransform(false));
    }
}