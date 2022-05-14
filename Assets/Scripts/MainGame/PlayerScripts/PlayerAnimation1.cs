using UnityEngine;

namespace MainGame.PlayerScripts
{
    public class PlayerAnimation1 : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
    
        // Boolean States hashes

        private static readonly int isStandingHash = Animator.StringToHash("isStanding");
        private static readonly int isWalkingHash = Animator.StringToHash("isWalking");
        private static readonly int isSprintingHash = Animator.StringToHash("isSprinting");
        private static readonly int justLandedHash = Animator.StringToHash("justLanded");

        public void Update()
        {
            bool isStanding = !Input.anyKey;
        
            bool goLeft = Input.GetKey(KeyCode.Q);
            bool goRight = Input.GetKey(KeyCode.D);
            bool goBack = Input.GetKey(KeyCode.S);
            bool goForward = Input.GetKey(KeyCode.Z);

            bool isWalking = goLeft || goRight || goBack || goForward;
            bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isWalking;
            isWalking &= !isSprinting;
        
            // Toggles "Stand" animation
            _animator.SetBool(isStandingHash, isStanding);
        
            // Toggles "Walk" animation
            _animator.SetBool(isWalkingHash, isWalking);

            // Toggles "Sprint" animation
            _animator.SetBool(isSprintingHash, isSprinting);
        }
    }
}
