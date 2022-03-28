using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class PlayerAnimation : MonoBehaviour
{
    // Scripts components
    
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;
    
    // Boolean States hashes

    private readonly int isStandingHash = Animator.StringToHash("isStanding");
    private readonly int isWalkingHash = Animator.StringToHash("isWalking");
    private readonly int isSprintingHash = Animator.StringToHash("isSprinting");

    public void EnableDeathAppearance()
    {
        return;
    }
    
    public void UpdateAnimationsBasic()
    {
        
        
        // Toggles "Stand" animation
        _animator.SetBool(isStandingHash, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Stand);
        
        // Toggles "Walk" animation
        _animator.SetBool(isWalkingHash, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Walk);

        // Toggles "Sprint" animation
        _animator.SetBool(isSprintingHash, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Sprint);
    }
}
