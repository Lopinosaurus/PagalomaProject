using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MainGame.PlayerScripts
{
    public partial class PlayerMovement : MonoBehaviour
    {
        #region Attributes

        private PlayerController PC;

        private bool WantsCrouchHold { get; set; }
        private bool WantsSprint { get; set; }

        // Movement speeds
        [Space, Header("Player speed settings"), SerializeField]
        private float currentSpeed;

        private const float SprintSpeed = 5f;
        private const float SprintBackSpeed = 3f;
        private const float CrouchSpeed = 1f;
        private const float WalkSpeed = 2f;
        private const float SmoothMoveValue = 10f;

        // Multipliers
        public bool isBushSlowingPlayer;
        public bool isWerewolfTransformedMult;
        private float _currentMultCoroutine = 1;

        private const float BushSlowSpeed = 2;
        
        private const float WerewolfMult = 1.15f;
        public const float AiStunMult = .5f;
        private const float BaseSpeedMult = 1;

        [HideInInspector] public int nbBushes;

        [Space, Header("Movement settings"), HideInInspector, SerializeField]
        public MovementState currentMovementState;

        public Vector2 localMoveAmountNormalized;
        private Vector2 _inputMoveRaw2D;

        // Gravity
        [Space, Header("Gravity settings")]
        private const float GravityForce = -9.81f;
        private const float DefaultGroundedGravityForce = -0.2f;
        private const float SlopeCompensationForce = 100;
        private const float SlopeForceLerpFactor = 10;
        private float _slopeRaySize = 1;
        public Vector3 upwardVelocity = Vector3.zero;

        // Ground check
        public bool grounded;
        public bool isSphereGrounded;

        private bool IsCcGrounded { get; set; }

        // Crouch & Hitboxes 
        [Space, Header("Player height settings"), SerializeField]
        private float crouchSmoothTime = 10;

        private struct HitboxesPos
        {
            public float HitboxHeight;
            public float CameraHeight;
            public float RenderShift;
        }

        private static HitboxesPos _standingVillager, _crouchingVillager, _standingWerewolf;
        private CharacterController _characterController;

        public enum MovementState
        {
            Idle,
            Crouch,
            Walk,
            Sprint
        }

        #endregion

        #region Unity methods

        private void Awake()
        {
            nbBushes = 0;
            PC = GetComponent<PlayerController>();
            _characterController = GetComponent<CharacterController>();

            _slopeRaySize = _characterController.radius * 1.1f;

            // Hitboxes
            float hitboxHeight = _characterController.height;
            _standingVillager.HitboxHeight = _standingWerewolf.HitboxHeight = hitboxHeight;
            _crouchingVillager.HitboxHeight = hitboxHeight * 0.7f;
            
            // Cam heights
            Vector3 camPos = PC.camHolder.transform.localPosition;
            _standingVillager.CameraHeight = camPos.y;
            _standingWerewolf.CameraHeight = 1.6f;
            _crouchingVillager.CameraHeight = 1;

            // Depths
            _standingVillager.RenderShift = PlayerController.BackShift;
            _crouchingVillager.RenderShift = -0.5f;
            _standingWerewolf.RenderShift = -1.5f;

            if (!PC.photonView.IsMine) Destroy(groundCheck);
        }

        private void Start()
        {
            // for the ZQSD movements
            PC.playerInput.actions["Move"].performed += OnPerformedMove;
            PC.playerInput.actions["Move"].canceled += _ => _inputMoveRaw2D = Vector2.zero;
            // for the Crouch button
            PC.playerInput.actions["Crouch"].performed += ctx => WantsCrouchHold = ctx.ReadValueAsButton();
            PC.playerInput.actions["Crouch"].canceled += ctx => WantsCrouchHold = ctx.ReadValueAsButton();
            // for the Sprint button
            PC.playerInput.actions["Sprint"].performed += ctx => WantsSprint = ctx.ReadValueAsButton();
            PC.playerInput.actions["Sprint"].canceled += ctx => WantsSprint = ctx.ReadValueAsButton();
            // for the Jump button
            PC.playerInput.actions["Jump"].performed += ctx => _wantsJump = ctx.ReadValueAsButton();
            PC.playerInput.actions["Jump"].canceled += ctx => _wantsJump = ctx.ReadValueAsButton();
        }

        private void OnPerformedMove(InputAction.CallbackContext ctx)
        {
            _inputMoveRaw2D = new Vector2
            {
                x = ctx.ReadValue<Vector2>().x,
                y = ctx.ReadValue<Vector2>().y
            };
        }

        #endregion

        #region Movements

        public void Move(float chosenDeltaTime)
        {
            Vector2 inputMoveNormalized2D = new Vector2
            {
                x = _inputMoveRaw2D.x,
                y = _inputMoveRaw2D.y
            }.normalized;

            // Updates the current movement type
            UpdateMovementState();

            // Updates gravity
            UpdateGravity(); // changes 'transformGravity'

            UpdateJump();

            // Updates the speed based on the MovementType
            UpdateSpeed();

            // Sets the new movement vector based on the inputs
            SmoothMoveAmount(inputMoveNormalized2D);

            // Removes moves if needed
            Vector3 currentMotion = Vector3.zero;

            // Applies direction
            if (!ShouldJumpFreezeControls)
                currentMotion = transform.TransformDirection(
                    new Vector3(
                        localMoveAmountNormalized.x,
                        0,
                        localMoveAmountNormalized.y));

            // Gravity
            currentMotion += upwardVelocity;

            // Time.deltaTime rounding
            currentMotion *= chosenDeltaTime;

            if (_characterController.enabled) _characterController.Move(currentMotion);
        }

        public void UpdateGrounded()
        {
            IsCcGrounded = _characterController.isGrounded;

            SetGroundedState(IsCcGrounded || isSphereGrounded);
        }

        private void UpdateMovementState()
        {
            MovementState newMovementState;
            
            if (CanCrouch()) newMovementState = MovementState.Crouch;
            else if (Vector2.zero == _inputMoveRaw2D) newMovementState = MovementState.Idle;
            else if (WantsSprint) newMovementState = MovementState.Sprint;
            else                newMovementState = MovementState.Walk;

            // ReSharper disable once RedundantCheckBeforeAssignment : it's a bug from resharper
            if (newMovementState != currentMovementState)
            {
                currentMovementState = newMovementState;
                PC.playerLook.ClampView(newMovementState);
            }
        }

        private bool CanCrouch() =>
            WantsCrouchHold && (!PC.playerAnimation.IsWerewolfEnabled && !ShouldJumpFreezeControls);
        
        private void UpdateGravity()
        {
            upwardVelocity.y += GravityForce * Time.deltaTime;

            if (ShouldJumpFreezeGravity)
            {
                upwardVelocity.y = 0;
            }
            else if (upwardVelocity.y <= 0)
            {
                if (OnSlope(out Vector3 hitNormal))
                {
                    float downwardForce = -SlopeCompensationForce * Vector3.Angle(Vector3.down, hitNormal);
                    downwardForce = Mathf.Clamp(downwardForce, -500, DefaultGroundedGravityForce);

                    upwardVelocity.y = Mathf.Lerp(upwardVelocity.y, downwardForce, Time.deltaTime * SlopeForceLerpFactor);
                }
                else if (IsCcGrounded)
                {
                    upwardVelocity.y = DefaultGroundedGravityForce;
                }
            }
        }

        private bool OnSlope(out Vector3 hitNormal)
        {
            var onSlope = false;
            hitNormal = Vector3.up;

            if (Physics.Raycast(transform.position + Vector3.up *.1f, Vector3.down * 10, out RaycastHit hit, _slopeRaySize)
                && hit.normal != Vector3.up) onSlope = true;

            return onSlope;
        }

        private void UpdateSpeed()
        {
            currentSpeed = currentMovementState switch
            {
                MovementState.Idle => 0f,
                MovementState.Crouch => CrouchSpeed,
                MovementState.Walk => WalkSpeed,
                MovementState.Sprint => SprintSpeed,
                _ => throw new System.ArgumentOutOfRangeException()
            };

            AdjustSpeedWithContext();
        }

        private void AdjustSpeedWithContext()
        {
            // Multipliers
            if (isWerewolfTransformedMult) currentSpeed *= WerewolfMult;
            currentSpeed *= _currentMultCoroutine;

            // Clamp speeds
            if (isBushSlowingPlayer) currentSpeed = Mathf.Clamp(currentSpeed, 0, BushSlowSpeed);
        }

        public void UpdateHitbox()
        {
            bool isWerewolf = PC.playerAnimation.IsWerewolfEnabled;
            
            // Chooses the new character controller settings
            HitboxesPos chosenHitboxesPos = currentMovementState switch
            {
                MovementState.Crouch => _crouchingVillager,
                _ when isWerewolf => _standingWerewolf,
                _ => _standingVillager
            };
                
            // Character controller modifier
            float smoothTime = Time.deltaTime * crouchSmoothTime;

            // Character center
            _characterController.height = Mathf.Lerp(_characterController.height, chosenHitboxesPos.HitboxHeight, smoothTime);
            // _characterController.height -= _characterController.skinWidth;
            Vector3 characterControllerCenter = _characterController.center;
            characterControllerCenter.y = _characterController.height * 0.5f;
            _characterController.center = characterControllerCenter;

            // Camera height modifier
            Vector3 localPosition = PC.camHolder.transform.localPosition;
            localPosition.y = Mathf.Lerp(localPosition.y, chosenHitboxesPos.CameraHeight, smoothTime);
            PC.camHolder.transform.localPosition = localPosition;

            // Camera render depth modifier
            PC.playerController.MoveRender(chosenHitboxesPos.RenderShift, PC.renders, smoothTime);
        }

        public void StartModifySpeed(float duration, float targetMultiplier, float startTime, float endTime)
        {
            startTime = Mathf.Clamp01(startTime);
            endTime = Mathf.Clamp(endTime, startTime, 1);
            targetMultiplier = targetMultiplier < 0 ? 0 : targetMultiplier;

            StartCoroutine(ModifySpeed(duration, targetMultiplier, startTime, endTime));
        }

        private IEnumerator ModifySpeed(float duration, float targetValue, float startTime, float endTime)
        {
            float timer = 0;
            float refMult = BaseSpeedMult;

            // First value
            while (timer <= duration * startTime)
            {
                float divider = duration * startTime;
                float progress = divider == 0 ? 1 : timer / divider;

                timer += Time.deltaTime;
                refMult = Mathf.Lerp(BaseSpeedMult, targetValue, progress);

                // Apply it
                _currentMultCoroutine = BaseSpeedMult * refMult;
                yield return null;
            }

            // Waits for endTime
            while (timer < duration * endTime)
            {
                timer += Time.deltaTime;

                yield return null;
            }

            // Second value
            float finalMult = BaseSpeedMult;
            while (timer < duration)
            {
                float progress = (timer - duration * endTime) / (duration * (1 - endTime));

                timer += Time.deltaTime;
                refMult = Mathf.Lerp(refMult, finalMult, progress);

                // Apply it
                _currentMultCoroutine = BaseSpeedMult * refMult;
                yield return null;
            }

            _currentMultCoroutine = BaseSpeedMult;
        }
        
        #endregion

        #region Setters

        private void SmoothMoveAmount(Vector2 moveDir)
        {
            Vector2 raw = moveDir * currentSpeed;

            // Caps the speed to not run backwards too fast
            if (raw.y < -SprintBackSpeed) raw.y = -SprintBackSpeed;

            Vector2 amount = Vector2.Lerp(localMoveAmountNormalized, raw, Time.deltaTime * SmoothMoveValue);

            localMoveAmountNormalized = amount;
        }

        private void SetGroundedState(bool grounded)
        {
            this.grounded = grounded;
        }

        #endregion
    }
}