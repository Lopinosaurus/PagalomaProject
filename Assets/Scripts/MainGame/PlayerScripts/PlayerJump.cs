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

        private PhotonView _photonView;

        [SerializeField] private Rigidbody groundCheck;

        private JumpState currentJumpState { get; set; } = JumpState.Still;

        [SerializeField] private JumpCollisionDetect[] obstaclesPresent;
        [SerializeField] private JumpCollisionDetect[] obstaclesAbsent;

        [SerializeField] private LayerMask characterLayer;

        private int _characterLayerValue;

        // Script components
        private PlayerAnimation _playerAnimation;
        private PlayerLook _playerLook;
        private bool AlreadyWantsJump;

        // Jump booleans
        private bool WantsJump;
        private const float jumpStrength = 4;

        private bool shouldJumpFreezeControls =>
            JumpState.MidVault == currentJumpState ||
            JumpState.HighVault == currentJumpState;

        private bool shouldJumpFreezeGravity =>
            JumpState.MidVault == currentJumpState ||
            JumpState.HighVault == currentJumpState;

        public bool SetJumpState(JumpState desired)
        {
            if (_photonView.IsMine)
            {
                if (desired != currentJumpState)
                {
                    currentJumpState = desired;

                    DisableCharacterController(desired);
                    _playerLook.LockViewJump(shouldJumpFreezeControls);
                    
                    _playerAnimation.SetAnimationJumpState((int)currentJumpState);

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
                    _characterController.enabled = true;
                    break;
                case JumpState.MidVault:
                    _characterController.enabled = false;
                    break;
                case JumpState.HighVault:
                    _characterController.enabled = false;
                    break;
            }
        }

        private void UpdateJump()
        {
            bool stateChanged = false;
            
            // Detects if the player can jump if wished
            if (JumpState.Still == currentJumpState)
                if (WantsJump)
                {
                    JumpState availableJump = GetAvailableJump();
                    stateChanged = SetJumpState(availableJump);
                    
                    switch (availableJump)
                    {
                        case JumpState.Still:
                            break;

                        case JumpState.SimpleJump:
                            _playerAnimation.StartSimpleJumpAnimation();
                            break;

                        case JumpState.MidVault:
                            _playerAnimation.StartMidVaultAnimation();
                            break;

                        case JumpState.HighVault:
                            break;
                    }
                }

            ManageJump(stateChanged);
        }

        private void ManageJump(bool stateChanged)
        {
            switch (currentJumpState)
            {
                case JumpState.SimpleJump when stateChanged:
                {
                    upwardVelocity.y = jumpStrength;
                    break;
                }
                
                case JumpState.SimpleJump:
                {
                    if (CollisionFlags.CollidedAbove == _characterController.collisionFlags)
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