using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

namespace MainGame.PlayerScripts
{
    public class FakePlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Avatar villagerAvatar;
        public Animator currentAnimator;
        
        // Components
        private PhotonView _photonView;
        private NavMeshAgent _agent;
        
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

        // Movement settings
        private Vector2 _velocity2D;
        private Vector2 _velocity2Draw;

        // Animation values
        public float VelocityX => currentAnimator.GetFloat(_velocityXHash);
        public float VelocityZ => currentAnimator.GetFloat(_velocityZHash);

        public float Velocity => new Vector2(Mathf.Sin(Mathf.Atan2(VelocityZ, VelocityX)) * VelocityZ,
            Mathf.Cos(Mathf.Atan2(VelocityZ, VelocityX)) * VelocityX).magnitude;
        
        private void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            _agent = GetComponent<NavMeshAgent>();
            
            if (!GetComponent<PhotonView>().IsMine) currentAnimator.applyRootMotion = false;
        }

        public void Update()
        {
            UpdateAnimationsBasic();
        }

        private void UpdateAnimationsBasic()
        {
            Vector3 agentVelocity = _agent.velocity;
            
            Vector3 velocity3D = transform.InverseTransformDirection(agentVelocity);
            _velocity2Draw = new Vector2
            {
                x = velocity3D.x,
                y = velocity3D.z
            };

            _velocity2D = Vector2.Lerp(_velocity2D, _velocity2Draw, 10f * Time.deltaTime);

            CorrectDiagonalMovement(true);

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
    }
}