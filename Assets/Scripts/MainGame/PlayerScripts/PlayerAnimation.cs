using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using UnityEditor.Animations;

public class PlayerAnimation : MonoBehaviour
{
    // Scripts components

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private Animator _animator;

    public Animator Animator => _animator;

    // Boolean States hashes

    private readonly int _isStandingHash = Animator.StringToHash("isStanding");
    private readonly int _isWalkingHash = Animator.StringToHash("isWalking");
    private readonly int _isSprintingHash = Animator.StringToHash("isSprinting");

    private readonly int _velocityXHash = Animator.StringToHash("VelocityX");
    private readonly int _velocityZHash = Animator.StringToHash("VelocityZ");
    private readonly int _blendHash = Animator.StringToHash("Blend");
    
    public void EnableDeathAppearance()
    {
        return;
    }
    
    public void UpdateAnimationsBasic()
    {
        Vector3 movementVector = new Vector3
        {
            x = _playerMovement.moveAmountRaw.x,
            z = _playerMovement.moveAmountRaw.z
        };
        
        // Sets the velocity in X to the CharacterController X velocity
        _animator.SetFloat(_velocityXHash, movementVector.x);
        
        // Sets the velocity in Z to the CharacterController Z velocity
        _animator.SetFloat(_velocityZHash, movementVector.z);
        
        // Sets the blend value to the magnitude of the Vector2 input
        _animator.SetFloat(_blendHash, 0.5f);
        
        // Toggles "Stand" animation
        _animator.SetBool(_isStandingHash, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Stand);
        
        // Toggles "Walk" animation
        _animator.SetBool(_isWalkingHash, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Walk);

        // Toggles "Sprint" animation
        _animator.SetBool(_isSprintingHash, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Sprint);
    }
}
