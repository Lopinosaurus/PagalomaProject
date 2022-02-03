using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class PlayerAnimation : MonoBehaviour
{
    private Rigidbody rb;
    private PhotonView PV;

    // Declaring player's appearances objects
    [Space]
    [Header("Player's poses")]
    [SerializeField] private GameObject playerStandingPose = null;
    [SerializeField] private GameObject playerCrouchingPose = null;
    [SerializeField] private GameObject playerSprintingPose = null;

    // Player Capsule Colliders
    [Space]
    [Header("Player's hitboxes")]
    [SerializeField] private GameObject playerStandingHitBox = null;
    [SerializeField] private GameObject playerCrouchingHitBox = null;
    [SerializeField] private GameObject playerWalkingHitBox = null;
    [SerializeField] private GameObject playerSprintingHitBox = null;

    void Awake() // Don't touch !
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        if (playerSprintingHitBox == null) Debug.LogWarning("Oups");
    }

    private void Start() // Don't touch !
    {
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }

        // Player's appearances
        playerStandingPose.SetActive(true);
        playerCrouchingPose.SetActive(false);
        playerSprintingPose.SetActive(false);

        // Player's hitboxes
        playerStandingHitBox.SetActive(true);
        playerCrouchingHitBox.SetActive(false);
        playerWalkingHitBox.SetActive(false);
        playerSprintingHitBox.SetActive(false);
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
        switch (_currentMovementType)
        {
            case PlayerController.MovementTypes.Stand:
                playerStandingHitBox.SetActive(true);
                playerCrouchingHitBox.SetActive(false);
                playerWalkingHitBox.SetActive(false);
                playerSprintingHitBox.SetActive(false);
                return;

            case PlayerController.MovementTypes.Crouch:
                playerStandingHitBox.SetActive(false);
                playerCrouchingHitBox.SetActive(true);
                playerWalkingHitBox.SetActive(false);
                playerSprintingHitBox.SetActive(false);
                return;

            case PlayerController.MovementTypes.Walk:
                playerStandingHitBox.SetActive(false);
                playerCrouchingHitBox.SetActive(false);
                playerWalkingHitBox.SetActive(true);
                playerSprintingHitBox.SetActive(false);
                return;

            case PlayerController.MovementTypes.Sprint:
                playerStandingHitBox.SetActive(false);
                playerCrouchingHitBox.SetActive(false);
                playerWalkingHitBox.SetActive(false);
                playerSprintingHitBox.SetActive(true);
                return;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
