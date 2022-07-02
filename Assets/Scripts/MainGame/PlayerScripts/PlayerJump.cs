using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public partial class PlayerMovement
    {
        public enum JumpState
        {   
            Still,
            SimpleJump,
            MidVault,
            HighVault
        }

        [SerializeField] private Rigidbody groundCheck;

        private JumpState CurrentJumpState { get; set; } = JumpState.Still;

        [SerializeField] private JumpCollisionDetect[] obstaclesPresent;
        [SerializeField] private JumpCollisionDetect[] obstaclesAbsent;

        [SerializeField] private LayerMask characterLayer;

        private int _characterLayerValue;

        // Jump booleans
        private bool _wantsJump;
        [SerializeField, Range(6, 50)] private float jumpStrength = 4;

        private bool ShouldJumpFreezeControls =>
            JumpState.MidVault == CurrentJumpState ||
            JumpState.HighVault == CurrentJumpState;

        private bool ShouldJumpFreezeGravity =>
            JumpState.MidVault == CurrentJumpState ||
            JumpState.HighVault == CurrentJumpState;

        public bool SetJumpState(JumpState desired)
        {
            if (PC.photonView.IsMine)
            {
                if (desired != CurrentJumpState)
                {
                    CurrentJumpState = desired;

                    DisableCharacterController(desired);
                    PC.playerLook.ClampView(CurrentJumpState);
                    PC.playerLook.LockViewJump(ShouldJumpFreezeControls);
                    PC.playerAnimation.SetAnimationJumpState((int)CurrentJumpState);

                    return true;
                }
            }

            return false;
        }

        private void DisableCharacterController(JumpState desired)
        {
            switch (desired)
            {
                case JumpState.Still:
                    PC.characterController.enabled = true;
                    break;
                case JumpState.MidVault:
                    PC.characterController.enabled = false;
                    break;
                case JumpState.HighVault:
                    PC.characterController.enabled = false;
                    break;
            }
        }

        private void UpdateJump()
        {
            bool stateChanged = false;
            
            // Detects if the player can jump if wished
            if (JumpState.Still == CurrentJumpState)
                if (_wantsJump)
                {
                    JumpState availableJump = GetAvailableJump();
                    stateChanged = SetJumpState(availableJump);
                    
                    switch (availableJump)
                    {
                        case JumpState.Still:
                            break;

                        case JumpState.SimpleJump:
                            PC.playerAnimation.StartSimpleJumpAnimation();
                            break;

                        case JumpState.MidVault:
                            PC.playerAnimation.StartMidVaultAnimation();
                            break;

                        case JumpState.HighVault:
                            break;
                    }
                }

            ManageJump(stateChanged);
        }

        private void ManageJump(bool stateChanged)
        {
            switch (CurrentJumpState)
            {
                case JumpState.SimpleJump when stateChanged:
                {
                    upwardVelocity.y = jumpStrength;
                    break;
                }
                
                case JumpState.SimpleJump:
                {
                    if (CollisionFlags.CollidedAbove == PC.characterController.collisionFlags)
                        SetJumpState(JumpState.Still);
                    break;
                }
            }
        }

        private JumpState GetAvailableJump()
        {
            // MidVault
            bool canMidVault =
                // Check if there room to jump
                !obstaclesAbsent.Any(j => j.IsColliding) &&
                // Check if there is an obstacle to vault over
                obstaclesPresent.All(j => j.IsColliding);

            if (canMidVault) return JumpState.MidVault;


            // Simple Jump
            if (grounded) return JumpState.SimpleJump;

            return JumpState.Still;
        }
    }
}