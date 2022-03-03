using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

[RequireComponent(typeof(PlayerController))]

public class PlayerAnimation : MonoBehaviour
{
    // Movement components
    private Rigidbody _rigidBody;
    private CharacterController _characterController;
    

    // Network component
    private PhotonView _photonView;


    // Declaring player's appearances objects
    [Space]
    [Header("Player's poses")]
    [SerializeField] private GameObject playerStandingPose = null;
    [SerializeField] private GameObject playerCrouchingPose = null;
    [SerializeField] private GameObject playerSprintingPose = null;

    // Player Smooth Crouch variables
    [Space] [Header("Smooth Crouch variables")] [SerializeField]
    private float crouchSpeed = 0.3f;
    private float standingHeight = 2f; // In "Start", will be set to the height of the "StandardHitbox" capsule
    private float crouchingHeight = 1f; // In "Start", will be set to half the height of the "StandardHitbox" capsule
    [SerializeField] private Transform playerCamera = null;

    void Awake() // Don't touch !
    {
        _rigidBody = GetComponent<Rigidbody>();
        _characterController = GetComponent<CharacterController>();
        _photonView = GetComponent<PhotonView>();
    }

    private void Start() // Don't touch !
    {
        // Player's appearances
        playerStandingPose.SetActive(true);
        playerCrouchingPose.SetActive(false);
        playerSprintingPose.SetActive(false);

        // Player's height variables
        standingHeight = _characterController.height;
        crouchingHeight = standingHeight / 2f;
    }

    public void UpdateAppearance(PlayerMovement.MovementTypes _currentMovementType)
    {
        playerStandingPose.SetActive(_currentMovementType == PlayerMovement.MovementTypes.Stand || _currentMovementType == PlayerMovement.MovementTypes.Walk); 
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
        if (Math.Abs(_characterController.height - desiredHeight) > 0)
        {
            AdjustHeight(desiredHeight);
            camPosition.y = _characterController.height;
        }
        
        playerCamera.transform.localPosition = camPosition;

        #endregion
    }

    private void AdjustHeight(float height)
    {
        // Calculates the current center of the player
        float center = height / 2f;

        _characterController.height = Mathf.LerpUnclamped(_characterController.height, height, crouchSpeed);
        _characterController.center = Vector3.LerpUnclamped(_characterController.center, new Vector3(0, center, 0), crouchSpeed);
    }
}
