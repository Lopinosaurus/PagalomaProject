using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Spy : Villager
    {
        [SerializeField] private SkinnedMeshRenderer PlayerRender;
        public override void UpdateActionText()
        {
            if (_photonView.IsMine)
            {
                if (hasCooldown == false && isAlive) actionText.text = "Press E to Activate Invisibility";
                else actionText.text = "";
            }
        }

         public override void UseAbility()
        {
            if (isAlive && VoteMenu.Instance.isNight) BecomeInvisible();
        }

        private void BecomeInvisible()
        {
            if (!VoteMenu.Instance.isNight) return;
            Debug.Log("E pressed and you are a Spy, you gonna be invisible");
            if (!hasCooldown)
            {
                hasCooldown = true;
                _photonView.RPC(nameof(RPC_BecomeInvisible), RpcTarget.Others);
                StartCoroutine(UpdateInvisibility());
                UpdateActionText();
            }
            else
            {
                Debug.Log("[-] Can't become Invisible: You have a Cooldown");
            }
        }
        IEnumerator UpdateInvisibility () {
            PlayerRender.enabled = false;
            yield return new WaitForSeconds(25);
            PlayerRender.enabled = true;
        }

        [PunRPC]
        public void RPC_BecomeInvisible()
        {
            if (RoomManager.Instance.localPlayer is Werewolf)
            {
                StartCoroutine(UpdateInvisibility());
            }
        }
    }
}
