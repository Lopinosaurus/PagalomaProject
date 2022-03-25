using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

[RequireComponent(typeof(PlayerController))]

public class PlayerAnimation : MonoBehaviour
{
    // Movement component
    private PlayerMovement _playerMovement;

    // Network component
    private PhotonView _photonView;

    // Declaring player's appearances objects
    [Space]
    [Header("Player's poses")]
    [SerializeField] private GameObject playerStandingPose;
    [SerializeField] private GameObject playerCrouchingPose;
    [SerializeField] private GameObject playerSprintingPose;
    [SerializeField] private GameObject playerDeathPose;

    // Player Smooth Crouch variables
    [Space] [Header("Smooth Crouch variables")]
    [SerializeField] private Transform _playerCameraRender;

    private void Awake() // Don't touch !
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _photonView = GetComponent<PhotonView>();
    }

    private void Start() // Don't touch !
    {
        // Player's appearances
        playerStandingPose.SetActive(true);
        playerCrouchingPose.SetActive(false);
        playerSprintingPose.SetActive(false);
        playerDeathPose.SetActive(false);
    }

    public void EnableDeathAppearance()
    {
        playerStandingPose.SetActive(false);
        playerCrouchingPose.SetActive(false);
        playerSprintingPose.SetActive(false);
        playerDeathPose.SetActive(true);
    }
    
    public void UpdateAppearance()
    {
        PlayerMovement.MovementTypes currentMovementType = _playerMovement.currentMovementType;
        
        playerStandingPose.SetActive(currentMovementType == PlayerMovement.MovementTypes.Stand ||
                                     currentMovementType == PlayerMovement.MovementTypes.Walk); 
        playerCrouchingPose.SetActive(PlayerMovement.MovementTypes.Crouch == currentMovementType);
        playerSprintingPose.SetActive(PlayerMovement.MovementTypes.Sprint == currentMovementType);
    }
}
