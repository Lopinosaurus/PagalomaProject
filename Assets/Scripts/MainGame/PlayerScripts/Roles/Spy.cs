using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Spy : Villager
    {
        private const float InvisibilityDuration = 25;
        private bool IsInvisible { get; set; }

        protected override AtMessage GetAtMessage()
        {
            if (ArePowerAndCooldownValid)
            {
                if (IsInvisible) return AtMessage.Clear;
                return AtMessage.PowerReadyToUse;
            }
            return AtMessage.PowerOnCooldown;
        }

        private new void Awake()
        {
            base.Awake();
            
            AtMessageDict = new()
            {
                {AtMessage.PowerReadyToUse, $"{RebindSystem.mainActionInputName}: Become invisible"},
                {AtMessage.PowerOnCooldown, "Can't become invisible until next night"},
                {AtMessage.Clear, ""}
            };
        }

        public override void UseAbility()
        {
            if (!isAlive) return;
            TriggerInvisible();
        }

        private void TriggerInvisible()
        {
            Debug.Log("E pressed and you are a Spy, you gonna be invisible");
            if (ArePowerAndCooldownValid)
            {
                PlayerController.powerTimer.Set(InvisibilityDuration);
                StartCoroutine(UpdateInvisibility());
            }
            else
            {
                Debug.Log("[-] Can't become Invisible: You have a Cooldown");
            }
        }

        private IEnumerator UpdateInvisibility()
        {
            ModifyInvisibility(true);
            PlayerController.photonView.RPC(nameof(RPC_ModifyInvisibility), RpcTarget.Others, true);
            UpdateActionText(GetAtMessage());
            
            yield return new WaitUntil((() => PlayerController.powerTimer.IsZero));
            
            ModifyInvisibility(false);
            PlayerController.photonView.RPC(nameof(RPC_ModifyInvisibility), RpcTarget.Others, false);
            UpdateActionText(GetAtMessage());
        }

        // TODO Improve visuals
        private void ModifyInvisibility(bool isBecomingInvisible)
        {
            IsInvisible = isBecomingInvisible;
            PlayerController.villagerSkinnedMeshRenderer.enabled = !isBecomingInvisible;
        }

        public override void Die()
        {
            base.Die();
            ModifyInvisibility(false);
            PlayerController.photonView.RPC(nameof(RPC_ModifyInvisibility), RpcTarget.Others, false);
        }

        [PunRPC]
        private void RPC_ModifyInvisibility(bool isBecomingInvisible)
        {
            if (RoomManager.Instance.localPlayer is Werewolf) ModifyInvisibility(isBecomingInvisible);
        }
    }
}