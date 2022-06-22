using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public class PlayerAnimation : MonoBehaviour
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
        private Vector2 _velocity2Draw;
        public bool IsWerewolfEnabled => currentAnimator.avatar == werewolfAvatar;

        // Animation values
        public float VelocityX => currentAnimator.GetFloat(_velocityXHash);
        public float VelocityZ => currentAnimator.GetFloat(_velocityZHash);

        public float Velocity => new Vector2(Mathf.Sin(Mathf.Atan2(VelocityZ, VelocityX)) * VelocityZ,
            Mathf.Cos(Mathf.Atan2(VelocityZ, VelocityX)) * VelocityX).magnitude;
        
        // Animations
        
        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _photonView = GetComponent<PhotonView>();

            currentAnimator = GetComponent<Animator>();
            foreach (SetJumpState rj in currentAnimator.GetBehaviours<SetJumpState>())
                rj.playerMovement = _playerMovement;

            _werewolfLayerIndex = currentAnimator.GetLayerIndex("WerewolfLayer");

            if (!GetComponent<PhotonView>().IsMine) currentAnimator.applyRootMotion = false;
        }

        public void UpdateAnimationsBasic()
        {
            Vector3 velocity3D = transform.InverseTransformDirection(_playerMovement.characterController.velocity);
            _velocity2Draw = new Vector2
            {
                x = velocity3D.x,
                y = velocity3D.z
            };

            _velocity2D = Vector2.Lerp(_velocity2D, _velocity2Draw, 10f * Time.deltaTime);

            CorrectDiagonalMovement(true);

            // Toggles "Crouch" animation
            currentAnimator.SetBool(_isCrouchingHash,
                _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Crouch);

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

            float magnitude = _velocity2Draw.magnitude;

            float velocityX = Mathf.Clamp(Mathf.Abs(Mathf.Tan(angleX) * magnitude), 0, magnitude)
                               * (_velocity2Draw.y > 0 ? 1 : -1);
            float velocityY = Mathf.Clamp(Mathf.Abs(Mathf.Tan(angleY) * magnitude), 0, magnitude)
                              * (_velocity2Draw.x > 0 ? 1 : -1);

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

            Debug.Log("jum^ped");

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