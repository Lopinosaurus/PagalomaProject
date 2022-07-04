using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Spy : Villager
    {
        private const float _invisibilityDuration = 25;
        private bool isInvisible { get; set; }

        protected new Dictionary<ATMessage, string> ATMessageDict = new Dictionary<ATMessage, string>();

        public override void UpdateActionText(ATMessage message = ATMessage.Clear)
        {
            if (PlayerController.photonView.IsMine)
            {
                if (PlayerController.powerTimer.IsNotZero && isAlive) ActionText.text = "Press E to Activate Invisibility";
                else ActionText.text = "";
            }
        }

        public override void UseAbility()
        {
            if (!CanUseAbilityGeneric()) return;
            
            TriggerInvisible();
        }

        private void TriggerInvisible()
        {
            if (!VoteMenu.Instance.IsNight) return;
            
            Debug.Log("E pressed and you are a Spy, you gonna be invisible");
            if (PlayerController.powerCooldown.IsZero && PlayerController.powerTimer.IsNotZero)
            {
                PlayerController.powerTimer.Reset();
                StartCoroutine(UpdateInvisibility());
                UpdateActionText();
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
            
            yield return new WaitForSeconds(_invisibilityDuration);
            
            if (isInvisible) ModifyInvisibility(false);
            PlayerController.photonView.RPC(nameof(RPC_ModifyInvisibility), RpcTarget.Others, false);
        }

        // TODO Improve visuals
        private void ModifyInvisibility(bool isBecomingInvisible)
        {
            isInvisible = isBecomingInvisible;
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