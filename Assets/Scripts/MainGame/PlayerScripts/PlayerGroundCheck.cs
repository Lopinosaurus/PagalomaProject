using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerGroundCheck : MonoBehaviour
{
    private PlayerMovement _playerMovement;
    [SerializeField] private GameObject player;

    private void Awake() => _playerMovement = player.GetComponent<PlayerMovement>();

    #region ONLY WORKS WITH RIGIDBODY - NO RIGIDBODY ATM
    
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
    
    #endregion

    #region not needed ATM
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == _playerMovement.gameObject)
            return;
        Debug.Log("Collision detected !");
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
 
    #endregion
}
