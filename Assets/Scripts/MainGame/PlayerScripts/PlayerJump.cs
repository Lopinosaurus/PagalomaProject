using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace MainGame.PlayerScripts
{
    public partial class PlayerMovement
    {
        // Jump values
        private bool WantsJump;
        private bool AlreadyWantsJump;

        [SerializeField] private Rigidbody jumpCollider;

        public enum JumpState
        {
            Still,
            SimpleJump,
            MidVault,
            HighVault
        }

        private bool shouldFreezeControlsJump =>
            JumpState.MidVault == currentJumpState ||
            JumpState.HighVault == currentJumpState;
        private bool shouldFreezeGravityJump =>
            JumpState.SimpleJump == currentJumpState ||
            JumpState.MidVault == currentJumpState ||
            JumpState.HighVault == currentJumpState;

       private static JumpState currentJumpState = JumpState.Still;
       public JumpState CurrentJumpState => currentJumpState;
       
        private PlayerAnimation _playerAnimation;
        private PlayerLook _playerLook;
        [SerializeField] private JumpCollisionDetect[] obstaclesPresent;
        [SerializeField] private JumpCollisionDetect[] obstaclesAbsent;
        [SerializeField] private LayerMask characterLayer;
        internal int _characterLayerValue;
        public List<Collider> ignoredJumpedColliders = new List<Collider>();
        private readonly List<Collider> collidersStopIgnoringVaultMid = new List<Collider>();
        private static PhotonView _photonView;

        public static void SetJumpState(JumpState desired)
         {
             if (!_photonView.IsMine) return;
             
             currentJumpState = desired;
         }

        private void UpdateJump()
         {
             if (grounded && WantsJump) upwardVelocity.y = 2.2f;
             
             return;
            
            // End jumping
            if (JumpState.Still == currentJumpState)
            {
                // Reset cam movement
                _playerLook.LockViewJump(false);

                // Stops ignoring them
                foreach (Collider jumpedCollider in collidersStopIgnoringVaultMid)
                    Physics.IgnoreCollision(_characterController, jumpedCollider, false);
            }

            // Gets what jump type is available
            JumpState availableJumpState = GetAvailableJump();

            // Tries to jump is not already jumping and wants to jump
            if (WantsJump && JumpState.Still == currentJumpState)
                switch (availableJumpState)
                {
                    // Simple Jump
                    case JumpState.SimpleJump when !AlreadyWantsJump && grounded:
                        currentJumpState = JumpState.SimpleJump;

                        // Starts the animation
                        _playerAnimation.StartSimpleJumpAnimation(true);
                        // Prevents the cam from turning too much
                        _playerLook.LockViewJump(false);
                        break;

                    // Mid vault
                    case JumpState.MidVault:

                        currentJumpState = JumpState.MidVault;

                        // Ignore colliders
                        collidersStopIgnoringVaultMid.Clear();
                        foreach (Collider jumpedCollider in ignoredJumpedColliders.Where(jumpedCollider =>
                                     null != jumpedCollider))
                        {
                            Physics.IgnoreCollision(_characterController, jumpedCollider, true);
                            // Sets them to stop ignoring them next
                            collidersStopIgnoringVaultMid.Add(jumpedCollider);
                        }

                        // Starts the animation
                        _playerAnimation.StartMidVaultAnimation(true);
                        // Prevents the cam from turning too much
                        _playerLook.LockViewJump(true);
                        break;

                    // High vault
                    case JumpState.HighVault:
                        break;
                }

            if (WantsJump) AlreadyWantsJump = true;
        }

        private JumpState GetAvailableJump()
        {
            // Check for MidVault
            var canMidVault =
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