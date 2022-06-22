using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Spy : Villager
    {
        [SerializeField] private SkinnedMeshRenderer playerRender;
        private readonly float _invisibilityDuration = 25;

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
            
            BecomeInvisible();
        }

        private void BecomeInvisible()
        {
            if (!VoteMenu.Instance.IsNight) return;
            Debug.Log("E pressed and you are a Spy, you gonna be invisible");
            if (PowerCooldown.IsZero && PowerTimer.IsNotZero)
            {
                PowerTimer.Reset();
                PhotonView.RPC(nameof(RPC_BecomeInvisible), RpcTarget.Others);
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
            // TODO Improve visuals
            playerRender.enabled = false;
            yield return new WaitForSeconds(_invisibilityDuration);
            playerRender.enabled = true;
        }

        [PunRPC]
        public void RPC_BecomeInvisible()
        {
            if (RoomManager.Instance.localPlayer is Werewolf) StartCoroutine(UpdateInvisibility());
        }
    }
}