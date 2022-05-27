using System;
using System.Collections;
using System.Collections.Generic;
using MainGame;
using MainGame.PlayerScripts.Roles;
using UnityEngine;

public class DetectCollision : MonoBehaviour
{
    [SerializeField] private Werewolf _werewolf;
    [SerializeField] private Seer _seer;
    [SerializeField] private Priest _priest;
    
    private void OnTriggerEnter(Collider other)
    {
        if (VoteMenu.Instance != null)
        {
            if (!VoteMenu.Instance.isDay && !VoteMenu.Instance.isFirstDay)
            {
                if (_werewolf.isActive && _werewolf.isAlive) _werewolf.UpdateTarget(other, true);
                if (_seer.isActive && _seer.isAlive) _seer.UpdateTarget(other, true);
                if (_priest.isActive && _priest.isAlive) _priest.UpdateTarget(other, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (VoteMenu.Instance != null)
        {
            if (!VoteMenu.Instance.isDay && !VoteMenu.Instance.isFirstDay)
            {
                if (_werewolf.isActive && _werewolf.isAlive) _werewolf.UpdateTarget(other, false);
                if (_seer.isActive && _seer.isAlive) _seer.UpdateTarget(other, false);
                if (_priest.isActive && _priest.isAlive) _priest.UpdateTarget(other, false);
            }
        }
    }
}
