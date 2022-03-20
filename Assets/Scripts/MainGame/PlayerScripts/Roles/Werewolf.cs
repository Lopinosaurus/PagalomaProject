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
        [SerializeField] private Collider potentialTarget;
        [SerializeField] private DetectCollision _detectCollision;

        public Werewolf(string username, string color) : base(username, color)
        {
            this.hasCooldown = false;
            this.target = null;
        }
        
        public void UpdateTarget(Collider other)
        {
            if (null == other)
            {
                target = null;
                return;
            };
            
            Debug.Log("In OnTriggerEnter");
            Debug.Log("other.tag = "+other.tag);
            if (other.CompareTag("Player"))
            {
                Villager tempTarget;
                if (other.TryGetComponent<Villager>(out tempTarget))
                {
                    if (this.roleName == "werewolf") // always true
                    {
                        if (tempTarget.roleName == "werewolf") return;
                        else
                        {
                            target = tempTarget;
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

