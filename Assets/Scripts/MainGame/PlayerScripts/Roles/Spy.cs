using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
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
                if (hasCooldown == false) actionText.text = "Press E to Activate Invisibility";
                else actionText.text = "";
            }
        }

         public override void UseAbility()
        {
            BecomeInvisible();
        }

        private void BecomeInvisible()
        {
            Debug.Log("E pressed and you are a Spy, you gonna be invisible");
            if (!hasCooldown)
            {
                hasCooldown = true;
                _photonView.RPC("RPC_BecomeInvisible", RpcTarget.Others);
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
            yield return new WaitForSeconds(10);
            PlayerRender.enabled = true;
        }

        [PunRPC]
        public void RPC_BecomeInvisible()
        {
            StartCoroutine(UpdateInvisibility());
        }
    }
}
