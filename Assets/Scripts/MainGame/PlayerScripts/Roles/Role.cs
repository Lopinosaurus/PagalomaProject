using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Role : PlayerController
    {
       public bool isAlive;
       public string username;
       public string color;
       public Role vote;

       public Role(string username, string color)
       {
           this.isAlive = true;
           this.username = username;
           this.color = color;
           this.vote = null;
       }

       public void Die()
       {
           throw new NotImplementedException();
       }
    }
}
