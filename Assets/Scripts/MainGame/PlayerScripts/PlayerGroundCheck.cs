using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    private PlayerController PC;
    [SerializeField] private GameObject player;

    void Awake()
    {
        PC = player.GetComponent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PC.gameObject)
            return;
        PC.SetGroundedState(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == PC.gameObject)
            return;
        PC.SetGroundedState(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == PC.gameObject)
            return;
        PC.SetGroundedState(true);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == PC.gameObject)
            return;
        PC.SetGroundedState(true);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == PC.gameObject)
            return;
        PC.SetGroundedState(false);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject == PC.gameObject)
            return;
        PC.SetGroundedState(true);
    }
}
