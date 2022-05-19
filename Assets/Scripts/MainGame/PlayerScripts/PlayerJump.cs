using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public partial class PlayerMovement
    {
        // Jump values
        private bool WantsJump { get; set; }
        [Space]
        [Header("Player jump settings")]
        private const float JumpForce = 1f;
        private const float JumpCompensation = 1;
        public bool isJumping;
        private PlayerAnimation _playerAnimation;
        private PlayerLook _playerLook;
        private float _jumpTimer = 0;
        [SerializeField] private JumpCollisionDetect[] obstaclesPresent;
        [SerializeField] private JumpCollisionDetect[] obstaclesAbsent;
        [SerializeField] private LayerMask characterLayer;
        private int _characterLayerValue;
        public List<Collider> ignoredJumpedColliders = new List<Collider>();
        private List<Collider> collidersStopIgnoring = new List<Collider>();
        [SerializeField] private JumpCollisionDetect[] jumpCollidersDetect;

        public void UpdateJump()
        {
            bool canJump = CanJump();
            Debug.Log("Can jump = " + canJump);
            if (WantsJump && !isJumping && canJump)
            {
                _jumpTimer = 0;
                isJumping = true;
                
                // Ignore colliders
                collidersStopIgnoring.Clear();
                foreach (Collider jumpedCollider in ignoredJumpedColliders.Where(jumpedCollider => jumpedCollider != null))
                {
                    Physics.IgnoreCollision(_characterController, jumpedCollider, true);
                    // Sets them to stop ignoring them next
                    collidersStopIgnoring.Add(jumpedCollider);
                }
                
                _playerAnimation.JumpAnimation(true);
                _playerLook.canTurnSides = false;
            }

            if (isJumping)
            {
                if (_jumpTimer > 1)
                {
                    isJumping = false;
                    _playerAnimation.JumpAnimation(false);
                    _playerLook.canTurnSides = true;
                    
                    // Stops ignoring them
                    foreach (var jumpedCollider in collidersStopIgnoring)
                    {
                        Physics.IgnoreCollision(_characterController, jumpedCollider, false);
                    }
                }
                else
                {
                    _jumpTimer += Time.deltaTime;
                }
            }
        }

        private bool CanJump()
        {
            // Check if there room to jump
            if (obstaclesAbsent.Any(j => j.IsColliding))
                return false;
            // Check if there is an obstacle to vault over
            if (obstaclesPresent.Any(j => !j.IsColliding))
                return false;

            return true;
        }
    }
}