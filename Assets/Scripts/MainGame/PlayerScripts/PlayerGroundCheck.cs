using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    private PlayerMovement _playerMovement;
    [SerializeField] private GameObject player;

    private void Awake() => _playerMovement = player.GetComponent<PlayerMovement>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _playerMovement.gameObject)
            return;
        _playerMovement.SetGroundedState(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == _playerMovement.gameObject)
            return;
        _playerMovement.SetGroundedState(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == _playerMovement.gameObject)
            return;
        _playerMovement.SetGroundedState(true);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == _playerMovement.gameObject)
            return;
        _playerMovement.SetGroundedState(true);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == _playerMovement.gameObject)
            return;
        _playerMovement.SetGroundedState(false);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject == _playerMovement.gameObject)
            return;
        _playerMovement.SetGroundedState(true);
    }
}
