using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.PlayerScripts.Roles;
using UnityEngine;

public class DetectCollision : MonoBehaviour
{
    private Collider previousCollider = null;
    [SerializeField] private Werewolf _werewolf;
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("name of the Collision collider is: " + collision.collider.name);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other != previousCollider)
        {
            _werewolf.UpdateTarget(other);
            Debug.Log("name of the Trigger collider is: " + other.name);

        }
        previousCollider = other;
    }

    private void OnTriggerExit(Collider other)
    {
        _werewolf.UpdateTarget(null);
        previousCollider = null;
    }
}
