using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.PlayerScripts.Roles;
using UnityEngine;

public class DetectCollision : MonoBehaviour
{
    [SerializeField] private Werewolf _werewolf;
    [SerializeField] private Seer _seer;
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[+] Collision detected with: " + other.name);
        if (_werewolf.isActive) _werewolf.UpdateTarget(other, true);
        if (_seer.isActive) _seer.UpdateTarget(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_werewolf.isActive) _werewolf.UpdateTarget(other, false);
        if (_seer.isActive) _seer.UpdateTarget(other, false);
    }
}
