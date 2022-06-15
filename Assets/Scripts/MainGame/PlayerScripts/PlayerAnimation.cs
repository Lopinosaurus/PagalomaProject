using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public class PlayerAnimation : MonoBehaviour
    {
        // Trigger States hashes
        private static readonly int _midVaultHash = Animator.StringToHash("MidVault");
        private static readonly int _simpleJumpHash = Animator.StringToHash("SimpleJump");
        private static readonly int _deathHash = Animator.StringToHash("Death");
        private static readonly int _attackHash = Animator.StringToHash("Attack");
        [SerializeField] private Avatar _villagerAvatar;
        [SerializeField] private Avatar _werewolfAvatar;
        public Animator CurrentAnimator;

        // Boolean States hashes
        private readonly int _isCrouchingHash = Animator.StringToHash("isCrouching");

        // Float States hashes
        private readonly int _velocityXHash = Animator.StringToHash("VelocityX");
        private readonly int _velocityZHash = Animator.StringToHash("VelocityZ");

        private PhotonView _photonView;

        // Scripts components
        private PlayerMovement _playerMovement;

        // Layer hashes
        private int _WerewolfLayerIndex;

        // Movement settings
        private Vector2 velocity2D;
        private Vector2 velocity2Draw;
        public bool isWerewolfEnabled => CurrentAnimator.avatar == _werewolfAvatar;

        // Animation values
        public float velocityX => CurrentAnimator.GetFloat(_velocityXHash);
        public float velocityZ => CurrentAnimator.GetFloat(_velocityZHash);

        public float velocity => new Vector2(Mathf.Sin(Mathf.Atan2(velocityZ, velocityX)) * velocityZ,
            Mathf.Cos(Mathf.Atan2(velocityZ, velocityX)) * velocityX).magnitude;

        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _photonView = GetComponent<PhotonView>();

            CurrentAnimator = GetComponent<Animator>();
            foreach (SetJumpState rj in CurrentAnimator.GetBehaviours<SetJumpState>())
                rj.PlayerMovement = _playerMovement;

            _WerewolfLayerIndex = CurrentAnimator.GetLayerIndex("WerewolfLayer");

            if (!GetComponent<PhotonView>().IsMine) CurrentAnimator.applyRootMotion = false;
        }

        public void UpdateAnimationsBasic()
        {
            Vector3 velocity3D = transform.InverseTransformDirection(_playerMovement._characterController.velocity);
            velocity2Draw = new Vector2
            {
                x = velocity3D.x,
                y = velocity3D.z
            };

            velocity2D = Vector2.Lerp(velocity2D, velocity2Draw, 10f * Time.deltaTime);

            CorrectDiagonalMovement(true);

            // Toggles "Crouch" animation
            CurrentAnimator.SetBool(_isCrouchingHash,
                _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Crouch);

            // Sets the velocity in X to the CharacterController X velocity
            CurrentAnimator.SetFloat(_velocityXHash, velocity2D.x);

            // Sets the velocity in Z to the CharacterController Z velocity
            CurrentAnimator.SetFloat(_velocityZHash, velocity2D.y);
        }

        private void CorrectDiagonalMovement(bool perform)
        {
            if (!perform) return;

            float angleX = -Vector2.SignedAngle(velocity2D, Vector2.right) * Mathf.Deg2Rad;
            float angleY = -Vector2.SignedAngle(velocity2D, Vector2.up) * Mathf.Deg2Rad;

            float magnitude = velocity2Draw.magnitude;

            float _velocityX = Mathf.Clamp(Mathf.Abs(Mathf.Tan(angleX) * magnitude), 0, magnitude)
                               * (velocity2Draw.y > 0 ? 1 : -1);
            float velocityY = Mathf.Clamp(Mathf.Abs(Mathf.Tan(angleY) * magnitude), 0, magnitude)
                              * (velocity2Draw.x > 0 ? 1 : -1);

            velocity2D = new Vector2(velocityY, _velocityX);
        }

        public void EnableDeathAppearance()
        {
            // Toggles "Dying" animation
            CurrentAnimator.SetTrigger(_deathHash);

            // Synchronises triggers
            _photonView.RPC(nameof(RPC_EnableDeathAppearance), RpcTarget.Others);
        }

        [PunRPC]
        private void RPC_EnableDeathAppearance()
        {
            CurrentAnimator.SetTrigger(_deathHash);
        }

        public void StartMidVaultAnimation()
        {
            // Toggles "MidVault" animation
            CurrentAnimator.SetTrigger(_midVaultHash);

            Debug.Log("jum^ped");

            // Synchronises triggers
            _photonView.RPC(nameof(RPC_MidVaultAnimation), RpcTarget.Others);
        }

        [PunRPC]
        private void RPC_MidVaultAnimation()
        {
            CurrentAnimator.SetTrigger(_midVaultHash);
        }

        public void StartSimpleJumpAnimation()
        {
            Debug.Log("started Jump");
            
            // Toggles "MidVault" animation
            CurrentAnimator.SetTrigger(_simpleJumpHash);


            // Synchronises triggers
            _photonView.RPC(nameof(RPC_SimpleJumpAnimation), RpcTarget.Others);
        }

        [PunRPC]
        private void RPC_SimpleJumpAnimation()
        {
            CurrentAnimator.SetTrigger(_simpleJumpHash);
        }

        public void EnableWerewolfAnimations(bool toWerewolf)
        {
            if (toWerewolf)
            {
                CurrentAnimator.avatar = _werewolfAvatar;
                CurrentAnimator.SetLayerWeight(_WerewolfLayerIndex, 1);
            }
            else
            {
                CurrentAnimator.avatar = _villagerAvatar;
                CurrentAnimator.SetLayerWeight(_WerewolfLayerIndex, 0);
            }
        }

        public void EnableWerewolfAttackAnimation()
        {
            // Toggles "Attack" animation
            CurrentAnimator.SetTrigger(_attackHash);

            // Synchronises triggers
            _photonView.RPC(nameof(RPC_WerewolfAttackAnimation), RpcTarget.Others);
        }

        [PunRPC]
        private void RPC_WerewolfAttackAnimation()
        {
            CurrentAnimator.SetTrigger(_attackHash);
        }
    }
}