using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public partial class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Avatar villagerAvatar, werewolfAvatar;
        private Animator _currentAnimator;
        private PlayerController PC;
        
        // Trigger States hashes
        private static readonly int MidVaultHash = Animator.StringToHash("MidVault");
        private static readonly int SimpleJumpHash = Animator.StringToHash("SimpleJump");
        private static readonly int DeathHash = Animator.StringToHash("Death");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        // Boolean States hashes
        private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
 
        // Int States hashes
        private static readonly int JumpStateHash = Animator.StringToHash("JumpState");

        // Float States hashes
        private static readonly int VelocityXHash = Animator.StringToHash("VelocityX");
        private static readonly int VelocityZHash = Animator.StringToHash("VelocityZ");

        // Layer hashes
        private int _werewolfLayerIndex;

        // Movement settings
        private Vector2 _velocity2D;
        private Vector2 _velocityRaw2D;
        private CharacterController _characterController;
        public bool IsWerewolfEnabled => _currentAnimator.avatar == werewolfAvatar;

        // Animation values
        public float VelocityX => _currentAnimator.GetFloat(VelocityXHash);
        public float VelocityZ => _currentAnimator.GetFloat(VelocityZHash);
        public float Velocity => new Vector2(Mathf.Sin(Mathf.Atan2(VelocityZ, VelocityX)) * VelocityZ,
            Mathf.Cos(Mathf.Atan2(VelocityZ, VelocityX)) * VelocityX).magnitude;
        
        private void Awake()
        {
            PC = GetComponent<PlayerController>();
            _characterController = GetComponent<CharacterController>();
            _currentAnimator = GetComponent<Animator>();
            
            // For each animation that has a SetJumpState script, we assign them the current PlayerMovement instance
            foreach (SetJumpState rj in _currentAnimator.GetBehaviours<SetJumpState>()) rj.playerMovement = PC.playerMovement;

            _werewolfLayerIndex = _currentAnimator.GetLayerIndex("WerewolfLayer");

            // This makes sure that other players' animator won't affect them with root motion, and instead only their
            // PhotonView will set their position.
            if (!GetComponent<PhotonView>().IsMine) _currentAnimator.applyRootMotion = false;
        }

        public void UpdateAnimationsBasic()
        {
            // We get the characterController's velocity
            Vector3 velocity3D = transform.InverseTransformDirection(_characterController.velocity);
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
            _currentAnimator.SetBool(IsCrouchingHash,
                PC.playerMovement.currentMovementState == PlayerMovement.MovementState.Crouch);

            // Sets the velocity in X to the CharacterController X velocity
            _currentAnimator.SetFloat(VelocityXHash, _velocity2D.x);

            // Sets the velocity in Z to the CharacterController Z velocity
            _currentAnimator.SetFloat(VelocityZHash, _velocity2D.y);
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
            _currentAnimator.SetTrigger(DeathHash);

            // Synchronises triggers
            PC.photonView.RPC(nameof(RPC_EnableDeathAppearance), RpcTarget.Others);
        }

        [PunRPC]
        private void RPC_EnableDeathAppearance()
        {
            _currentAnimator.SetTrigger(DeathHash);
        }

        public void StartMidVaultAnimation()
        {
            // Toggles "MidVault" animation
            _currentAnimator.SetTrigger(MidVaultHash);

            // Synchronises triggers
            PC.photonView.RPC(nameof(RPC_MidVaultAnimation), RpcTarget.Others);
        }

        [PunRPC]
        private void RPC_MidVaultAnimation()
        {
            _currentAnimator.SetTrigger(MidVaultHash);
        }

        public void StartSimpleJumpAnimation()
        {
            // Toggles "MidVault" animation
            _currentAnimator.SetTrigger(SimpleJumpHash);

            // Synchronises triggers
            PC.photonView.RPC(nameof(RPC_SimpleJumpAnimation), RpcTarget.Others);
        }

        [PunRPC]
        private void RPC_SimpleJumpAnimation()
        {
            _currentAnimator.SetTrigger(SimpleJumpHash);
        }

        [ContextMenu(nameof(EnableWerewolfAnimations), true)]
        public void EnableWerewolfAnimations(bool toWerewolf)
        {
            if (toWerewolf)
            {
                _currentAnimator.avatar = werewolfAvatar;
                _currentAnimator.SetLayerWeight(_werewolfLayerIndex, 1);
            }
            else
            {
                _currentAnimator.avatar = villagerAvatar;
                _currentAnimator.SetLayerWeight(_werewolfLayerIndex, 0);
            }
        }

        public void EnableWerewolfAttackAnimation()
        {
            // Toggles "Attack" animation
            _currentAnimator.SetTrigger(AttackHash);

            // Synchronises triggers
            PC.photonView.RPC(nameof(RPC_WerewolfAttackAnimation), RpcTarget.Others);
        }

        [PunRPC]
        private void RPC_WerewolfAttackAnimation()
        {
            _currentAnimator.SetTrigger(AttackHash);
        }

        public void SetAnimationJumpState(int state)
        {
            _currentAnimator.SetInteger(JumpStateHash, state);
        }
    }
}