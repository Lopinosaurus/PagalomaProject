using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Spy : Villager
    {
        private readonly float _invisibilityDuration = 25;
        public bool isInvisible { get; private set; }

        public override void UpdateActionText()
        {
            if (PhotonView.IsMine)
            {
                if (PowerTimer.IsNotZero && isAlive) ActionText.text = "Press E to Activate Invisibility";
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
            if (PowerCooldown.IsZero && PowerTimer.IsNotZero)
            {
                PowerTimer.Reset();
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
            PhotonView.RPC(nameof(RPC_ModifyInvisibility), RpcTarget.Others, true);
            
            yield return new WaitForSeconds(_invisibilityDuration);
            
            if (isInvisible) ModifyInvisibility(false);
            PhotonView.RPC(nameof(RPC_ModifyInvisibility), RpcTarget.Others, false);
        }

        // TODO Improve visuals
        private void ModifyInvisibility(bool isBecomingInvisible)
        {
            isInvisible = isBecomingInvisible;
            VillagerSkinnedMeshRenderer.enabled = !isBecomingInvisible;
        }

        public override void Die()
        {
            base.Die();
            ModifyInvisibility(false);
            PhotonView.RPC(nameof(RPC_ModifyInvisibility), RpcTarget.Others, false);
        }

        [PunRPC]
        private void RPC_ModifyInvisibility(bool isBecomingInvisible)
        {
            if (RoomManager.Instance.localPlayer is Werewolf) ModifyInvisibility(isBecomingInvisible);
        }
    }
}