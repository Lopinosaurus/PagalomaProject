using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MainGame.PlayerScripts.Roles;
using Photon.Pun;

public class PlayerAnimation : MonoBehaviour
{
    // Scripts components
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private Animator _animator;

    public Animator Animator => _animator;

    // Boolean States hashes
    private readonly int _isCrouchingHash = Animator.StringToHash("isCrouching");
    private readonly int _deathHash = Animator.StringToHash("Death");

    // Float States hashes
    private readonly int _velocityXHash = Animator.StringToHash("VelocityX");
    private readonly int _velocityZHash = Animator.StringToHash("VelocityZ");

    public void UpdateAnimationsBasic()
    {
        Vector3 movementVector = new Vector3
        {
            x = _playerMovement.localMoveAmountRaw.x,
            z = _playerMovement.localMoveAmountRaw.z
        };
        
        // Toggles "Crouch" animation
        _animator.SetBool(_isCrouchingHash, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Crouch);
        
        // Sets the velocity in X to the CharacterController X velocity
        _animator.SetFloat(_velocityXHash, movementVector.x);
        
        // Sets the velocity in Z to the CharacterController Z velocity
        _animator.SetFloat(_velocityZHash, movementVector.z);
    }

    public void EnableDeathAppearance()
    {
        // Toggles "Dying" animation
        _animator.SetTrigger(_deathHash);
    }
}
