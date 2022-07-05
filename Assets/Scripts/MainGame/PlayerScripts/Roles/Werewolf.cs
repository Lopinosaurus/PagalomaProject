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
        public static readonly string FriendlyRoleName = "Werewolf";

        protected override AtMessage GetAtMessage()
        {
            if (ArePowerAndCooldownValid)
            {
                if (targets.Count > 0)
                    if (isTransformed)
                        return AtMessage.PowerReadyToUse;
                    else
                        return AtMessage.PowerReadyToEnable;
                return AtMessage.Clear;
            }
            return AtMessage.PowerOnCooldown;
        }

        private new void Awake()
        {
            base.Awake();
            
            AtMessageDict = new()
            {
                {AtMessage.PowerReadyToUse, $"{RebindSystem.mainActionInputName}: Kill"},
                {AtMessage.PowerReadyToEnable, $"{RebindSystem.mainActionInputName}: Transform into a Werewolf"},
                {AtMessage.PowerOnCooldown, "Can't become a Werewolf until next night"},
                {AtMessage.Clear, ""}
            };
        }
        
        public List<Role> targets = new List<Role>();
        public bool isTransformed;

        public float werewolfPowerDuration = 60;
        public float afterAttackCooldown = 5;
        
        private const float EarlyTransformationTransitionDuration = 1;
        private const float LateTransformationTransitionDuration = 4;

        private float TotalTransformationTransitionDuration =>
            EarlyTransformationTransitionDuration + LateTransformationTransitionDuration;
        
        /// <summary>
        ///   <para>Adds or removes the targets list.</para>
        /// </summary>
        /// <param name="other">The collider that will be updated.</param>
        /// <param name="add">When true, adds the collider to the targets list, otherwise will try to remove it.</param>
        public override void UpdateTarget(Collider other, bool add)
        {
            if (isTransformed == false || other is not CharacterController || !other.CompareTag("Player")) return;

            // Adds or removes role
            if (other.TryGetComponent(out Role targetRole) && targetRole is not Werewolf)
            {
                if (add)
                {
                    targets.Add(targetRole);
                    Debug.Log("[+] Werewolf target added: " + targetRole.name);
                }
                else
                {
                    targets.Remove(targetRole);
                    Debug.Log("[-] Werewolf target removed: " + targetRole.name);
                }
            }

            // Updates the messages on screen
            UpdateActionText(GetAtMessage());
        }

        public override void UseAbility()
        {
            if (!CanUseAbilityGeneric())
            {
                Debug.LogError("you can NOT use your power");
                return;
            }
            
            Debug.LogError("you can use your power");
            
            if (isTransformed) KillTarget();
            else UpdateTransformation(true);
        }

        public void UpdateTransformation(bool goingToWerewolf)
        {
            // Avoids useless transformations is already in the desired state
            if (goingToWerewolf == isTransformed) return;
            
            if (PlayerController.photonView.IsMine) PlayerController.photonView.RPC(goingToWerewolf ? nameof(RPC_Transformation) : nameof(RPC_Detransformation), RpcTarget.Others);

            StartCoroutine(WerewolfTransform(goingToWerewolf));
        }

        private IEnumerator WerewolfTransform(bool goingToWerewolf)
        {
            // Particles to dissimulate werewolf transition
            GameObject particles = Instantiate(PlayerController.dissimulateParticles, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            particles.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            particles.transform.parent = transform;

            // Deactivate controls
            if (PlayerController.photonView.IsMine) PlayerController.playerInput.SwitchCurrentActionMap("UI");

            // Transformation
            isTransformed = goingToWerewolf;
            PlayerController.playerMovement.isWerewolfTransformedMult = goingToWerewolf;

            // Starts countdown
            if (goingToWerewolf)
            {
                PlayerController.powerTimer.Set(werewolfPowerDuration);
                PlayerController.powerTimer.Pause();
                PlayerController.powerTimer.Resume(TotalTransformationTransitionDuration);
            }
            else PlayerController.powerTimer.Reset();

            if (goingToWerewolf && PlayerController.photonView.IsMine) StartCoroutine(DeTransformationCoroutine());

            // Updates the messages on screen
            UpdateActionText(GetAtMessage());

            // Wait
            yield return new WaitForSeconds(EarlyTransformationTransitionDuration);

            // TODO improve visual transition
            PlayerController.villagerRender.SetActive(!goingToWerewolf);
            PlayerController.werewolfRender.SetActive(goingToWerewolf);

            // Changes the animator
            PlayerController.playerAnimation.EnableWerewolfAnimations(goingToWerewolf);

            // Wait for 4 seconds
            yield return new WaitForSeconds(LateTransformationTransitionDuration);

            // Reactivate controls
            if (PlayerController.photonView.IsMine) PlayerController.playerInput.SwitchCurrentActionMap("Player");
        }

        private IEnumerator DeTransformationCoroutine()
        {
            // Will bring the werewolf back to human when the power timer runs out
            yield return new WaitUntil(() => PlayerController.powerTimer.IsZero);

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

            Role target = targets[^1];

            // Cannot attack if target is dead
            if (target.isAlive == false)
            {
                Debug.Log("[-] Can't kill: Target is already dead");
                return;
            }

            // Cannot attack if target has shield
            if (target.isShielded)
            {
                RoomManager.Instance.UpdateInfoText("Kill attempt failed because the player has a shield!");
                return;
            }

            Debug.Log("E pressed and you are a Werewolf, you gonna kill someone");

            target.Die();
            targets.Remove(target);
            PlayerController.photonView.RPC(nameof(RPC_KillTarget), RpcTarget.Others, target.userId);

            PlayerController.playerAnimation.EnableWerewolfAttackAnimation();

            // Waits a few seconds before re-enabling attack
            PlayerController.powerCooldown.Set(afterAttackCooldown);
            // it also pauses the timer for the power
            PlayerController.powerTimer.Pause();
            PlayerController.powerTimer.Resume(afterAttackCooldown);
            // it also slows the Werewolf
            PlayerController.playerMovement.StartModifySpeed(afterAttackCooldown, 0.5f, 0.1f, 0.7f);

            UpdateActionText(GetAtMessage());
            
            if (target.isShielded)
            {
                RoomManager.Instance.UpdateInfoText("Kill attempt failed because the player has a shield!");
                return;
            }

            Debug.Log("E pressed and you are a Werewolf, you gonna kill someone");

            target.Die();
            targets.Remove(target);
            PlayerController.photonView.RPC(nameof(RPC_KillTarget), RpcTarget.Others, target.userId);

            PlayerController.playerAnimation.EnableWerewolfAttackAnimation();
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