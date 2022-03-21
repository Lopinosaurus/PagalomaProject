using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.PlayerScripts.Roles;
using UnityEngine;

public class DetectCollision : MonoBehaviour
{
    [SerializeField] private Werewolf _werewolf;

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("[+] Collision detected with: " + other.name);
        _werewolf.UpdateTarget(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        _werewolf.UpdateTarget(other, false);
    }
}
