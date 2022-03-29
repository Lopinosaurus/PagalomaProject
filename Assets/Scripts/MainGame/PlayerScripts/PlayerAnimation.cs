using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class PlayerAnimation : MonoBehaviour
{
    // Scripts components

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;
    
    // Boolean States hashes

    private readonly int isStandingHash = Animator.StringToHash("isStanding");
    private readonly int isWalkingHash = Animator.StringToHash("isWalking");
    private readonly int isSprintingHash = Animator.StringToHash("isSprinting");

    private readonly int velocityXHash = Animator.StringToHash("VelocityX");
    private readonly int velocityZHash = Animator.StringToHash("VelocityZ");
    private readonly int BlendHash = Animator.StringToHash("Blend");

    public void EnableDeathAppearance()
    {
        return;
    }
    
    public void UpdateAnimationsBasic()
    {
        // Sets the velocity in X to the CharacterController X velocity
        _animator.SetFloat(velocityXHash, _playerMovement.transformDirection.x);
        
        // Sets the velocity in Z to the CharacterController Z velocity
        _animator.SetFloat(velocityZHash, _playerMovement.transformDirection.z);
        
        // Sets the blend value to the magnitude of the Vector2 input
        _animator.SetFloat(BlendHash, _playerMovement.transformDirection.magnitude);
        
        // Toggles "Stand" animation
        _animator.SetBool(isStandingHash, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Stand);
        
        // Toggles "Walk" animation
        _animator.SetBool(isWalkingHash, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Walk);

        // Toggles "Sprint" animation
        _animator.SetBool(isSprintingHash, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Sprint);
    }
}
