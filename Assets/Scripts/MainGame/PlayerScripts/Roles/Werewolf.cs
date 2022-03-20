using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Werewolf : Role
    {
        private bool hasCooldown;
        private Role target;
        [SerializeField] private SphereCollider attackCollider;

        public Werewolf(string username, string color) : base(username, color)
        {
            this.hasCooldown = false;
            this.target = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("In OnTriggerEnter");
            Debug.Log("other.tag = "+other.tag);
            if (other.CompareTag("Player"))
            {
                Villager tempTaget;
                if (other.TryGetComponent<Villager>(out tempTaget))
                {
                    if (this.roleName == "werewolf")
                    {
                        if (tempTaget.roleName == "werewolf") return;
                        else
                        {
                            target = tempTaget;
                            Debug.Log(target.name);
                        }
                    }
                }
            }
        }

        public override void KillTarget()
        {
            if (!hasCooldown)
            {
                Debug.Log("In KillTarget");
                if (target != null && target.isAlive)
                {
                    transform.position = target.transform.position;
                    target.Die();
                    target = null;
                    hasCooldown = true;
                }
                else
                {
                    Debug.Log("No target");
                }
            }
        }
    }
}

