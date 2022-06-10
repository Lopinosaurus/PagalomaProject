using System.Linq;
using Photon.Pun;
using UnityEngine;


namespace MainGame.PlayerScripts
{
    public partial class PlayerMovement
    {
        // Script components
        private PlayerAnimation _playerAnimation;
        private PlayerLook _playerLook;
        private static PhotonView _photonView;
        
        // Jump booleans
        private bool WantsJump;
        private bool AlreadyWantsJump;

        [SerializeField] private Rigidbody groundCheck;

        public enum JumpState
        {
            Still,
            SimpleJump,
            MidVault,
            HighVault
        }

        private bool shouldJumpFreezeControls =>
            JumpState.MidVault == currentJumpState ||
            JumpState.HighVault == currentJumpState;
        private bool shouldJumpFreezeGravity =>
            JumpState.SimpleJump == currentJumpState ||
            JumpState.MidVault == currentJumpState ||
            JumpState.HighVault == currentJumpState;

        public JumpState currentJumpState = JumpState.Still;

       [SerializeField] private JumpCollisionDetect[] obstaclesPresent;
        [SerializeField] private JumpCollisionDetect[] obstaclesAbsent;
        
        [SerializeField] private LayerMask characterLayer;
        private int _characterLayerValue;

        public void SetJumpState(JumpState desired)
         {
             if (!_photonView.IsMine) return;

             if (desired != currentJumpState)
             {
                 DisableCharacterController(desired);
                 _playerLook.LockViewJump(JumpState.MidVault == desired || JumpState.HighVault == desired);
             }

             currentJumpState = desired;
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
            // Detects if the player can jump if wished
            if (JumpState.Still == currentJumpState)
            {
                if (WantsJump)
                {
                    JumpState availableJump = GetAvailableJump();

                    switch (availableJump)
                    {
                        case JumpState.Still:
                            break;
                    
                        case JumpState.SimpleJump:
                            SetJumpState(JumpState.SimpleJump);
                            _playerAnimation.StartSimpleJumpAnimation(true);
                            break;
                    
                        case JumpState.MidVault:
                            SetJumpState(JumpState.MidVault);
                            _playerAnimation.StartMidVaultAnimation(true);
                            break;
                    
                        case JumpState.HighVault:
                            SetJumpState(JumpState.HighVault);
                            break;
                    }

                }
            }

            ManageJump();
        }

        private void ManageJump()
        {
            switch (currentJumpState)
            {
                case JumpState.SimpleJump:
                {
                    if (_characterController.collisionFlags == CollisionFlags.Above)
                        SetJumpState(JumpState.Still);
                    break;
                }
            }
        }
        
        private JumpState GetAvailableJump()
        {
            // MidVault
            var canMidVault =
                // Check if there room to jump
                !obstaclesAbsent.Any(j => j.IsColliding) &&
                // Check if there is an obstacle to vault over
                obstaclesPresent.All(j => j.IsColliding);

            if (canMidVault) return JumpState.MidVault;

            
            // Simple Jump
            if (grounded)
            {
                return JumpState.SimpleJump;
            }

            return JumpState.Still;
        }
    }
}