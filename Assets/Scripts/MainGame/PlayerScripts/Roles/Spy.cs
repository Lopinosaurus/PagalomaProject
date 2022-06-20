using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Spy : Villager
    {
        [SerializeField] private SkinnedMeshRenderer PlayerRender;
        private readonly float invisibilityDuration = 25;

        public override void UpdateActionText()
        {
            if (_photonView.IsMine)
            {
                if (powerTimer.isNotZero && isAlive) actionText.text = "Press E to Activate Invisibility";
                else actionText.text = "";
            }
        }

        public override void UseAbility()
        {
            if (!CanUseAbilityGeneric()) return;
            
            BecomeInvisible();
        }

        private void BecomeInvisible()
        {
            if (!VoteMenu.Instance.isNight) return;
            Debug.Log("E pressed and you are a Spy, you gonna be invisible");
            if (powerCooldown.isZero && powerTimer.isNotZero)
            {
                powerTimer.Reset();
                _photonView.RPC(nameof(RPC_BecomeInvisible), RpcTarget.Others);
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
            PlayerRender.enabled = false;
            yield return new WaitForSeconds(invisibilityDuration);
            PlayerRender.enabled = true;
        }

        [PunRPC]
        public void RPC_BecomeInvisible()
        {
            if (RoomManager.Instance.localPlayer is Werewolf) StartCoroutine(UpdateInvisibility());
        }
    }
}