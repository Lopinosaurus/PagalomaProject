using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public partial class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Avatar villagerAvatar;
        [SerializeField] private Avatar werewolfAvatar;
        public Animator currentAnimator;
        
        // Scripts components
        private PlayerMovement _playerMovement;
        private PhotonView _photonView;
        
        // Trigger States hashes
        private static readonly int MidVaultHash = Animator.StringToHash("MidVault");
        private static readonly int SimpleJumpHash = Animator.StringToHash("SimpleJump");
        private static readonly int DeathHash = Animator.StringToHash("Death");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        // Boolean States hashes
        private readonly int _isCrouchingHash = Animator.StringToHash("IsCrouching");
 
        // Int States hashes
        private readonly int _jumpStateHash = Animator.StringToHash("JumpState");

        // Float States hashes
        private readonly int _velocityXHash = Animator.StringToHash("VelocityX");
        private readonly int _velocityZHash = Animator.StringToHash("VelocityZ");

        // Layer hashes
        private int _werewolfLayerIndex;

        // Movement settings
        private Vector2 _velocity2D;
        private Vector2 _velocityRaw2D;
        public bool IsWerewolfEnabled => currentAnimator.avatar == werewolfAvatar;

        // Animation values
        public float VelocityX => currentAnimator.GetFloat(_velocityXHash);
        public float VelocityZ => currentAnimator.GetFloat(_velocityZHash);
        public float Velocity => new Vector2(Mathf.Sin(Mathf.Atan2(VelocityZ, VelocityX)) * VelocityZ,
            Mathf.Cos(Mathf.Atan2(VelocityZ, VelocityX)) * VelocityX).magnitude;
        
        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _photonView = GetComponent<PhotonView>();
            currentAnimator = GetComponent<Animator>();
            
            // For each animation that has a SetJumpState script, we assign them the current PlayerMovement instance
            foreach (SetJumpState rj in currentAnimator.GetBehaviours<SetJumpState>()) rj.playerMovement = _playerMovement;

            _werewolfLayerIndex = currentAnimator.GetLayerIndex("WerewolfLayer");

            // This makes sure that other players' animator won't affect them with root motion, and instead only their
            // PhotonView will set their position.
            if (!GetComponent<PhotonView>().IsMine) currentAnimator.applyRootMotion = false;
        }

        public void UpdateAnimationsBasic()
        {
            // We get the characterController's velocity
            Vector3 velocity3D = transform.InverseTransformDirection(_playerMovement.characterController.velocity);
            _velocityRaw2D = new Vector2
            {
                x = velocity3D.x,
                y = velocity3D.z
            };

            // We smoothen the previous velocity with the new one so that animations transition smoothly too
            _velocity2D = Vector2.Lerp(_velocity2D, _velocityRaw2D, 10f * Time.deltaTime);

            // This makes sure that the animation doesn't slow down when the player is moving diagonally.
            // Indeed, diagonal movements have a norm equal to the speed value, but put on a 2D graph their axis to
            // axis values will be less than the speed value (e.g with forward right with a speed of 1 m/s will
            // give 0.7 on both the Y and X axis, because the speed vector was normalized).
            CorrectDiagonalMovement(true);

            // Toggles "Crouch" animation
            currentAnimator.SetBool(_isCrouchingHash,
                _playerMovement.currentMovementState == PlayerMovement.MovementState.Crouch);

            // Sets the velocity in X to the CharacterController X velocity
            currentAnimator.SetFloat(_velocityXHash, _velocity2D.x);

            // Sets the velocity in Z to the CharacterController Z velocity
            currentAnimator.SetFloat(_velocityZHash, _velocity2D.y);
        }

        private void CorrectDiagonalMovement(bool perform)
        {
            if (!perform) return;

            float angleX = -Vector2.SignedAngle(_velocity2D, Vector2.right) * Mathf.Deg2Rad;
            float angleY = -Vector2.SignedAngle(_velocity2D, Vector2.up) * Mathf.Deg2Rad;

            float magnitude = _velocityRaw2D.magnitude;

            float velocityX = Mathf.Clamp(Mathf.Abs(Mathf.Tan(angleX) * magnitude), 0, magnitude)
                               * (_velocityRaw2D.y > 0 ? 1 : -1);
            float velocityY = Mathf.Clamp(Mathf.Abs(Mathf.Tan(angleY) * magnitude), 0, magnitude)
                              * (_velocityRaw2D.x > 0 ? 1 : -1);

            _velocity2D = new Vector2(velocityY, velocityX);
        }

        public void EnableDeathAppearance()
        {
            // Toggles "Dying" animation
            currentAnimator.SetTrigger(DeathHash);

            // Synchronises triggers
            _photonView.RPC(nameof(RPC_EnableDeathAppearance), RpcTarget.Others);
        }

        [PunRPC]
        private void RPC_EnableDeathAppearance()
        {
            currentAnimator.SetTrigger(DeathHash);
        }

        public void StartMidVaultAnimation()
        {
            // Toggles "MidVault" animation
            currentAnimator.SetTrigger(MidVaultHash);

            // Synchronises triggers
            _photonView.RPC(nameof(RPC_MidVaultAnimation), RpcTarget.Others);
        }

        [PunRPC]
        private void RPC_MidVaultAnimation()
        {
            currentAnimator.SetTrigger(MidVaultHash);
        }

        public void StartSimpleJumpAnimation()
        {
            // Toggles "MidVault" animation
            currentAnimator.SetTrigger(SimpleJumpHash);

            // Synchronises triggers
            _photonView.RPC(nameof(RPC_SimpleJumpAnimation), RpcTarget.Others);
        }

        [PunRPC]
        private void RPC_SimpleJumpAnimation()
        {
            currentAnimator.SetTrigger(SimpleJumpHash);
        }

        [ContextMenu(nameof(EnableWerewolfAnimations), true)]
        public void EnableWerewolfAnimations(bool toWerewolf)
        {
            if (toWerewolf)
            {
                currentAnimator.avatar = werewolfAvatar;
                currentAnimator.SetLayerWeight(_werewolfLayerIndex, 1);
            }
            else
            {
                currentAnimator.avatar = villagerAvatar;
                currentAnimator.SetLayerWeight(_werewolfLayerIndex, 0);
            }
        }

        public void EnableWerewolfAttackAnimation()
        {
            // Toggles "Attack" animation
            currentAnimator.SetTrigger(AttackHash);

            // Synchronises triggers
            _photonView.RPC(nameof(RPC_WerewolfAttackAnimation), RpcTarget.Others);
        }

        [PunRPC]
        private void RPC_WerewolfAttackAnimation()
        {
            currentAnimator.SetTrigger(AttackHash);
        }

        public void SetAnimationJumpState(int state)
        {
            currentAnimator.SetInteger(_jumpStateHash, state);
        }
    }
}