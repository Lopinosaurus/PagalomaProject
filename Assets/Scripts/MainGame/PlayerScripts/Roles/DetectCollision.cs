using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.PlayerScripts.Roles;
using UnityEngine;

public class DetectCollision : MonoBehaviour
{
    public Collider previousCollider = null;
    [SerializeField] private Werewolf _werewolf;
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("name of the Collision collider is: " + collision.collider.name);
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("name of the Trigger collider is: " + other.name);
        if (other != previousCollider) _werewolf.UpdateTarget(other);
        previousCollider = other;
    }

    private void OnTriggerExit(Collider other)
    {
        _werewolf.UpdateTarget(null);
        previousCollider = null;
    }
}
