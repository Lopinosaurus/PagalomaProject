using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public partial class PlayerMovement
    {
        // Jump values
        private bool WantsJump { get; set; }
        public enum JumpState
        {
            Still,
            SimpleJump,
            MidVault,
            HighVault
        }

        private bool shouldJumpFreezeGravity =>
            JumpState.MidVault == currentJumpState || JumpState.HighVault == currentJumpState;
        public JumpState currentJumpState = JumpState.Still;
        
        private PlayerAnimation _playerAnimation;
        private PlayerLook _playerLook;
        [SerializeField] private JumpCollisionDetect[] obstaclesPresent;
        [SerializeField] private JumpCollisionDetect[] obstaclesAbsent;
        [SerializeField] private LayerMask characterLayer;
        internal int _characterLayerValue;
        public List<Collider> ignoredJumpedColliders = new List<Collider>();
        private List<Collider> collidersStopIgnoringVaultMid = new List<Collider>();
        private float jumpStrength = 2;

        // ReSharper disable once UnusedMember.Global
        public void DeactivateJumpBool()
        {
            currentJumpState = JumpState.Still;
        }

        public void UpdateJump()
        {
            // Gets what jump type is available
            JumpState availableJumpState = GetAvailableJump();
            
            // Tries to jump is not already jumping and wants to jump
            if (WantsJump && JumpState.Still == currentJumpState)
            {
                switch (availableJumpState)
                {
                    // Simple Jump
                    case JumpState.SimpleJump:
                        currentJumpState = JumpState.SimpleJump;
                        
                        upwardVelocity = Vector3.up * jumpStrength;
                        break;
                    
                    // Mid vault
                    case JumpState.MidVault:
                    {
                        currentJumpState = JumpState.MidVault;
                        
                        // Ignore colliders
                        collidersStopIgnoringVaultMid.Clear();
                        foreach (Collider jumpedCollider in ignoredJumpedColliders.Where(jumpedCollider => jumpedCollider != null))
                        {
                            Physics.IgnoreCollision(_characterController, jumpedCollider, true);
                            // Sets them to stop ignoring them next
                            collidersStopIgnoringVaultMid.Add(jumpedCollider);
                        }
                
                        // Starts the animation
                        _playerAnimation.StartVaultMidAnimation(true);
                        // Prevents the cam from turning too much
                        _playerLook.canTurnSides = false;
                        break;
                    }
                    
                    // High vault
                    case JumpState.HighVault:
                        break;
                }
            }

            // End jumping
            if (JumpState.Still == currentJumpState)
            {
                _playerAnimation.StartVaultMidAnimation(false);
                _playerLook.canTurnSides = true;
                
                // Stops ignoring them
                foreach (var jumpedCollider in collidersStopIgnoringVaultMid)
                {
                    Physics.IgnoreCollision(_characterController, jumpedCollider, false);
                }
            }
        }

        private JumpState GetAvailableJump()
        {
            // Check for MidVault
            bool canMidVault = 
            // Check if there room to jump
            !obstaclesAbsent.Any(j => j.IsColliding) &&
            // Check if there is an obstacle to vault over
            obstaclesPresent.All(j => j.IsColliding);

            if (canMidVault) return JumpState.MidVault;

            if (grounded) return JumpState.SimpleJump;
            
            return JumpState.Still;
        }
    }
}