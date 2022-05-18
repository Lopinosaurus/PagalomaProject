using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.Menus;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    
    public class Werewolf : Role
    {
        [SerializeField] private GameObject VillageRenderer;
        [SerializeField] private GameObject WereWolfRenderer;
        [SerializeField] private GameObject Particles;
        
        public List<Role> _targets = new List<Role>();
        public bool isTransformed = false;

        public override void UpdateActionText()
        {
            if (_photonView.IsMine)
            {
                if (isTransformed)
                {
                    if (_targets.Count > 0 && hasCooldown == false) actionText.text = "Press E to Kill";
                    else actionText.text = "";
                }
                else if (VoteMenu.Instance.isNight)
                {
                    if (hasCooldown == false) actionText.text = "Press E to Transform";
                    else actionText.text = "";
                }
            }
        }
         
        public void UpdateTarget(Collider other, bool add) // Add == true -> add target to targets list, otherwise remove target from targets
        {
            if (isTransformed == false) return;
            if (other.CompareTag("Player"))
            {
                Role tempTarget = null;
                foreach (Role role in other.GetComponents<Role>())
                {
                    if (role.enabled) tempTarget = role;
                }

                if (tempTarget != null && !(tempTarget is Werewolf))
                {
                    if (add)
                    {
                        _targets.Add(tempTarget);
                        Debug.Log("[+] Werewolf target added: "+tempTarget.name);
                    } else if (_targets.Contains(tempTarget))
                    {
                        _targets.Remove(tempTarget);
                        Debug.Log("[-] Werewolf target removed: "+tempTarget.name);
                    }
                }
            }
            UpdateActionText();
        }

        public override void UseAbility()
        {
            if (!hasCooldown)
            {
               if (isTransformed) KillTarget();
                else Transformation(); 
            }
        }

        private void Transformation()
        {
            if (_photonView.IsMine) _photonView.RPC("RPC_Transformation", RpcTarget.Others);
            if (VillageRenderer.activeSelf)
            {
                GameObject p =Instantiate(Particles, this.transform.position+new Vector3(0,1.1f,0), quaternion.identity);
                //p.transform.localRotation = quaternion.identity;
                p.transform.rotation = Quaternion.Euler(-90,0,0);
                StartCoroutine(TransformationAnimationCoroutine(1f));
            }
            Debug.Log("Werewolf Transformation");
            isTransformed = true;
            UpdateActionText();
            if (_photonView.IsMine) StartCoroutine(DeTransformationCoroutine(60));
        }

        private IEnumerator TransformationAnimationCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            VillageRenderer.SetActive(false);
            WereWolfRenderer.SetActive(true);
        }
        private IEnumerator DetransformationAnimationCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            VillageRenderer.SetActive(true);
            WereWolfRenderer.SetActive(false);
        }

        private IEnumerator DeTransformationCoroutine(int delay)
        {
            yield return new WaitForSeconds(delay);
            DeTransformation();
        }

        private void DeTransformation()
        {
             if (_photonView.IsMine) _photonView.RPC("RPC_DeTransformation", RpcTarget.Others);
             
             
             if (WereWolfRenderer.activeSelf)
             {
                 GameObject p =Instantiate(Particles, this.transform.position+new Vector3(0,1.1f,0), quaternion.identity);
                 //p.transform.localRotation = quaternion.identity;
                 p.transform.rotation = Quaternion.Euler(-90,0,0);
                 StartCoroutine(DetransformationAnimationCoroutine(1f));
             }
            Debug.Log("Werewolf DeTransformation");
            isTransformed = false;
            UpdateActionText();
        }
        
        private void KillTarget() // TODO: Add kill animation
        {
            Debug.Log("E pressed and you are a Werewolf, you gonna kill someone");
            if (!hasCooldown)
            {
                if (_targets.Count > 0)
                {
                    Role target = _targets[_targets.Count - 1];
                    if (target.isAlive == false)
                    {
                        Debug.Log("[-] Can't kill: Target is already dead");
                        return;
                    }
                    transform.position = target.transform.position;
                    hasCooldown = true;
                    UpdateActionText();
                    if (target.hasShield)
                    {
                        RoomManager.Instance.UpdateInfoText($"Kill attempt failed because the player has a shield!");
                        return;
                    }
                    target.Die();
                    _targets.Remove(target);
                    _photonView.RPC("RPC_KillTarget", RpcTarget.Others, target.userId);
                }
                else
                {
                    Debug.Log("[-] Can't kill: No target to kill");
                }
            }
            else
            {
                Debug.Log("[-] Can't kill: You have a Cooldown");
            }
        }

       
        
        [PunRPC]
        public void RPC_KillTarget(string userId) // TODO: Add kill animation
        {
            Role target = null;
            foreach (Role player in RoomManager.Instance.players) // Get target with corresponding userId
            {
                if (player.userId == userId) target = player;
            }

            if (target != null)
            {
                if (target.isAlive) target.Die();
                else Debug.Log($"[-] RPC_KillTarget({userId}): Can't kill, Target is already dead");
            }
            else Debug.Log($"[-] RPC_KillTarget({userId}): Can't kill, target = null");
        }

        [PunRPC]
        public void RPC_Transformation()
        {
            Transformation();
        }
        
        [PunRPC]
        public void RPC_DeTransformation()
        {
            DeTransformation();
        }
    }
}

