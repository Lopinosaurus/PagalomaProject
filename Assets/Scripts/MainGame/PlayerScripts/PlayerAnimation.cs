using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class PlayerAnimation : MonoBehaviour
{
    // Scripts components
    private PlayerMovement _playerMovement;
    private CharacterController _characterController;
    private PhotonView _photonView;
    [SerializeField] private Animator _animator;
    
    // Boolean States hashes

    private static readonly int isStanding = Animator.StringToHash("isStanding");
    private static readonly int isWalking = Animator.StringToHash("isWalking");
    private static readonly int isSprinting = Animator.StringToHash("isSprinting");

    private void Awake() // Don't touch !
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _characterController = GetComponent<CharacterController>();
        _photonView = GetComponent<PhotonView>();
    }

    public void EnableDeathAppearance()
    {

    }
    
    public void UpdateAnimationsBasic()
    {
        // Toggles "Stand" animation
        _animator.SetBool(isStanding, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Stand);
        
        // Toggles "Walk" animation
        _animator.SetBool(isWalking, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Walk);

        // Toggles "Sprint" animation
        _animator.SetBool(isSprinting, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Sprint);
    }
}
