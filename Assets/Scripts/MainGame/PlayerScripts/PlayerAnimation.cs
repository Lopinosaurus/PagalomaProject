using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

[RequireComponent(typeof(PlayerController))]

public class PlayerAnimation : MonoBehaviour
{
    private PlayerController PC;
    private Rigidbody RB;
    private PhotonView PV;

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
    private CapsuleCollider CC = null;
    
    // Player Smooth Crouch variables
    [Space] [Header("Smooth Crouch variables")] [SerializeField]
    private float crouchSpeed = 0.3f;
    private float standingHeight = 2f; // In "Start", will be set to the height of the "StandardHitbox" capsule
    private float crouchingHeight = 1f; // In "Start", will be set to half the height of the "StandardHitbox" capsule
    [SerializeField] private Transform playerCamera = null;

    void Awake() // Don't touch !
    {
        PC = GetComponent<PlayerController>();
        RB = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        CC = playerStandardHitBox.GetComponent<CapsuleCollider>();
    }

    private void Start() // Don't touch !
    {
        // Player's appearances
        playerStandingPose.SetActive(true);
        playerCrouchingPose.SetActive(false);
        playerSprintingPose.SetActive(false);
        
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(RB);
            
            return;
        }
        
        // Player's hitboxes
        playerStandardHitBox.SetActive(true);
        
        // Player's height variables
        standingHeight = CC.height;
        crouchingHeight = standingHeight / 2f;
    }

    public void UpdateAppearance(PlayerController.MovementTypes _currentMovementType)
    {
        switch (_currentMovementType)
        {
            case PlayerController.MovementTypes.Stand:
                playerStandingPose.SetActive(true);
                playerCrouchingPose.SetActive(false);
                playerSprintingPose.SetActive(false);
                return;

            case PlayerController.MovementTypes.Crouch:
                playerStandingPose.SetActive(false);
                playerCrouchingPose.SetActive(true);
                playerSprintingPose.SetActive(false);
                return;

            case PlayerController.MovementTypes.Walk:
                playerStandingPose.SetActive(true);
                playerCrouchingPose.SetActive(false);
                playerSprintingPose.SetActive(false);
                return;

            case PlayerController.MovementTypes.Sprint:
                playerStandingPose.SetActive(false);
                playerCrouchingPose.SetActive(false);
                playerSprintingPose.SetActive(true);
                return;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void UpdateHitbox(PlayerController.MovementTypes _currentMovementType)
    {
        // Updates player's hitbox about crouch
        #region Crouch management
        
        float desiredHeight = standingHeight;
        if (PlayerController.MovementTypes.Crouch == _currentMovementType) desiredHeight = crouchingHeight;

        Vector3 camPosition = playerCamera.transform.localPosition;
        if (CC.height != desiredHeight)
        {
            AdjustHeight(desiredHeight);
            camPosition.y = CC.height;
        }
        
        playerCamera.transform.localPosition = camPosition;

        #endregion
        
        
        
    }

    private void AdjustHeight(float height)
    {
        // Calculates the current center of the player
        float center = height / 2f;

        CC.height = Mathf.LerpUnclamped(CC.height, height, crouchSpeed);
        CC.center = Vector3.LerpUnclamped(CC.center, new Vector3(0, center, 0), crouchSpeed);
    }
}
