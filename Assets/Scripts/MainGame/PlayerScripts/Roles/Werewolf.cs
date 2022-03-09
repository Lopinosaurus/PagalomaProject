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
        
        public Werewolf(string username, string color) : base(username, color)
        {
            this.hasCooldown = false;
            this.target = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
               // TODO 
            }
        }

        public void KillTarget()
        {
            if (!hasCooldown)
            {
                if (target != null && target.isAlive)
                {
                    transform.position = target.transform.position;
                    target.Die();
                    target = null;
                    hasCooldown = true;
                }
            }
        }
    }
}

