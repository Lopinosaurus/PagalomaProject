using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

[RequireComponent(typeof(PlayerController))]

public class PlayerAnimation : MonoBehaviour
{
    private PlayerController _playerController;
    private PlayerMovement _playerMovement;
    private Rigidbody _rigidBody;
    private PhotonView _photonView;

    // Declaring player's appearances objects
    [Space]
    [Header("Player's poses")]
    [SerializeField] private GameObject playerStandingPose = null;
    [SerializeField] private GameObject playerCrouchingPose = null;
    [SerializeField] private GameObject playerSprintingPose = null;

    // Player Capsule Collider
    [Space]
    [Header("Player's hitboxes")]
    [SerializeField] private GameObject playerStandardHitBox = null;
    private CapsuleCollider _capsuleCollider = null;
    
    // Player Smooth Crouch variables
    [Space] [Header("Smooth Crouch variables")] [SerializeField]
    private float crouchSpeed = 0.3f;
    private float standingHeight = 2f; // In "Start", will be set to the height of the "StandardHitbox" capsule
    private float crouchingHeight = 1f; // In "Start", will be set to half the height of the "StandardHitbox" capsule
    [SerializeField] private Transform playerCamera = null;

    void Awake() // Don't touch !
    {
        _playerController = GetComponent<PlayerController>();
        _playerMovement = GetComponent<PlayerMovement>();
        _rigidBody = GetComponent<Rigidbody>();
        _photonView = GetComponent<PhotonView>();
        
        _capsuleCollider = playerStandardHitBox.GetComponent<CapsuleCollider>();
    }

    private void Start() // Don't touch !
    {
        // Player's appearances
        playerStandingPose.SetActive(true);
        playerCrouchingPose.SetActive(false);
        playerSprintingPose.SetActive(false);
        
        // Player's hitboxes
        playerStandardHitBox.SetActive(true);
        
        if (!_photonView.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(_rigidBody);
            
            return;
        }

        // Player's height variables
        standingHeight = _capsuleCollider.height;
        crouchingHeight = standingHeight / 2f;
    }

    public void UpdateAppearance(PlayerMovement.MovementTypes _currentMovementType)
    {
        playerStandingPose.SetActive(PlayerMovement.MovementTypes.Stand == _currentMovementType); 
        playerCrouchingPose.SetActive(PlayerMovement.MovementTypes.Crouch == _currentMovementType);
        playerSprintingPose.SetActive(PlayerMovement.MovementTypes.Sprint == _currentMovementType);
    
        //TODO: re-enable network
        // Network sync: hitbox
        /*
        _photonView.RPC("RPC_UpdateAppearance", RpcTarget.Others, _playerMovement.currentMovementType);
    */
    }

    public void UpdateHitbox(PlayerMovement.MovementTypes _currentMovementType)
    {
        // Updates player's hitbox about crouch
        #region Crouch management
        
        float desiredHeight = standingHeight;
        if (PlayerMovement.MovementTypes.Crouch == _currentMovementType) desiredHeight = crouchingHeight;

        Vector3 camPosition = playerCamera.transform.localPosition;
        if (Math.Abs(_capsuleCollider.height - desiredHeight) > 0)
        {
            AdjustHeight(desiredHeight);
            camPosition.y = _capsuleCollider.height;
        }
        
        playerCamera.transform.localPosition = camPosition;

        #endregion
        
        
        
    }

    private void AdjustHeight(float height)
    {
        // Calculates the current center of the player
        float center = height / 2f;

        _capsuleCollider.height = Mathf.LerpUnclamped(_capsuleCollider.height, height, crouchSpeed);
        _capsuleCollider.center = Vector3.LerpUnclamped(_capsuleCollider.center, new Vector3(0, center, 0), crouchSpeed);
    }
}
