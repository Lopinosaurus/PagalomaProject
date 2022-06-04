using System;
using System.Collections;
using MainGame.PlayerScripts.Roles;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MainGame.PlayerScripts
{
    public partial class PlayerMovement : MonoBehaviour
    {
        #region Attributes

        // External GameObjects and components
        [SerializeField] public GameObject cameraHolder;

        // Player Controller & Controls
        private PlayerInput _playerInput;
        private PlayerController _playerController;

        // Movement components
        [HideInInspector] public CharacterController _characterController;

        private bool WantsCrouchHold { get; set; }
        private bool WantsSprint { get; set; }

        // Movement speeds
        [Space] [Header("Player speed settings")] [SerializeField]
        private float currentSpeed;

        private const float SprintSpeed = 5f;
        private const float SprintBackSpeed = 3f;
        private const float CrouchSpeed = 1f;
        private const float WalkSpeed = 2f;
        private const float SmoothMoveValue = 10f;

        // Multipliers
        public bool isBushMult;
        public bool isWerewolfMult;

        private const float BushMult = 0.4f;
        public const float AiStunMult = 0.5f;
        private float currentMultCoroutine = 1;
        private const float WerewolfMult = 1.15f;
        private const float baseSpeedMult = 1;

        [HideInInspector] public int nbBushes;

        [Space] [Header("Movement settings")] [HideInInspector] [SerializeField]
        public MovementTypes currentMovementType = MovementTypes.Stand;

        public Vector2 localMoveAmountNormalized;
        private Vector2 _inputMoveRaw2D;

        // Gravity
        [Space] [Header("Gravity settings")] private const float GravityForce = -9.81f;
        public Vector3 upwardVelocity = Vector3.zero;

        // Ground check
        private float raySize = 0.1f;
        public bool grounded;
        private readonly float slopeCompensationForce = 100f;
        public bool isSphereGrounded { get; set; }
        private bool isCCgrounded { get; set; }

        // Crouch & Hitboxes 
        [Space] [Header("Player height settings")] [SerializeField]
        private float crouchSmoothTime = 10;

        private float _standingHitboxHeight;
        private float _crouchedHitboxHeight;

        private float _standingCameraHeightVillager;
        private float _standingCameraHeightWerewolf;
        private float _crouchedCameraHeight;

        private float _standingShiftVillager;
        private float _crouchedShiftVillager;
        private float _shiftWerewolf;

        public enum MovementTypes
        {
            Stand,
            Crouch,
            Walk,
            Sprint
        }

        #endregion

        #region Unity methods

        private void Awake()
        {
            nbBushes = 0;
            _playerInput = GetComponent<PlayerInput>();
            _playerAnimation = GetComponent<PlayerAnimation>();
            _playerLook = GetComponent<PlayerLook>();
            _playerController = GetComponent<PlayerController>();
            _photonView = GetComponent<PhotonView>();

            _characterController = GetComponentInChildren<CharacterController>();
            _characterLayerValue = (int) Mathf.Log(characterLayer.value, 2);

            raySize = _characterController.radius * 1.1f;

            // Hitboxes
            var hitboxHeight = _characterController.height;
            _standingHitboxHeight = hitboxHeight;
            _crouchedHitboxHeight = hitboxHeight * 0.7f;

            // Cam heights
            Vector3 camPos = cameraHolder.transform.localPosition;
            _standingCameraHeightVillager = camPos.y;
            _standingCameraHeightWerewolf = 1.6f;
            _crouchedCameraHeight = 1;

            // Profs
            _standingShiftVillager = _playerController.backShift;
            _crouchedShiftVillager = 0.5f;
            _shiftWerewolf = 1.5f;
        }

        private void Start()
        {
            // For glitch cc abuse
            if (RoomManager.Instance != null) StartCoroutine(IgnorePlayerCollisions());
            // for the ZQSD movements
            _playerInput.actions["Move"].performed += OnPerformedMove;
            _playerInput.actions["Move"].canceled += _ => _inputMoveRaw2D = Vector2.zero;
            // for the Crouch button
            _playerInput.actions["Crouch"].performed += ctx => WantsCrouchHold = ctx.ReadValueAsButton();
            _playerInput.actions["Crouch"].canceled += ctx => WantsCrouchHold = ctx.ReadValueAsButton();
            // for the Sprint button
            _playerInput.actions["Sprint"].performed += ctx => WantsSprint = ctx.ReadValueAsButton();
            _playerInput.actions["Sprint"].canceled += ctx => WantsSprint = ctx.ReadValueAsButton();
            // for the Jump button
            _playerInput.actions["Jump"].performed += ctx => WantsJump = ctx.ReadValueAsButton();
            _playerInput.actions["Jump"].canceled += ctx => AlreadyWantsJump = WantsJump = ctx.ReadValueAsButton();
        }

        private IEnumerator IgnorePlayerCollisions()
        {
            yield return new WaitUntil(() => RoomManager.Instance.localPlayer != null);

            foreach (Role role in RoomManager.Instance.players)
                Physics.IgnoreCollision(_characterController,
                    role.gameObject.GetComponent<CharacterController>(), true);
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
            var inputMoveNormalized2D = new Vector2
            {
                x = _inputMoveRaw2D.x,
                y = _inputMoveRaw2D.y
            }.normalized;

            // Updates the grounded boolean state
            UpdateGrounded();

            // Updates the current movement type
            UpdateMovementState();

            // Updates gravity
            UpdateGravity(); // changes 'transformGravity'

            // Updates the speed based on the MovementType
            UpdateSpeed();

            // Sets the new movement vector based on the inputs
            SmoothMoveAmount(inputMoveNormalized2D);

            // Applies direction
            Vector3 currentMotion = transform.TransformDirection(new Vector3(localMoveAmountNormalized.x,
                0,
                localMoveAmountNormalized.y));

            // Removes moves if needed
            if (shouldFreezeControlsJump) currentMotion *= 0;
             
            currentMotion += upwardVelocity;
            
            // Time.deltaTime rounding
            currentMotion *= chosenDeltaTime;
            
            // Move
            _characterController.Move(currentMotion);
        }

        private void UpdateGrounded()
        {
            isCCgrounded = _characterController.isGrounded;

            SetGroundedState(isCCgrounded || isSphereGrounded);
        }

        private void UpdateMovementState()
        {
            if (WantsCrouchHold && !_playerAnimation.isWerewolfEnabled && !shouldFreezeControlsJump)
                currentMovementType = MovementTypes.Crouch;
            else if (Vector2.zero == _inputMoveRaw2D)
                currentMovementType = MovementTypes.Stand;
            else if (WantsSprint)
                currentMovementType = MovementTypes.Sprint;
            else
                currentMovementType = MovementTypes.Walk;
        }

        private void UpdateGravity()
        {
            upwardVelocity.y += GravityForce * Time.deltaTime;

            if (shouldFreezeGravityJump)
            {
                upwardVelocity.y = 0;
            }
            else if (grounded)
            {
                if (OnSlope())
                {
                    float downwardForce = -slopeCompensationForce;
                    downwardForce = Mathf.Clamp(downwardForce, -500, -0.2f);

                    upwardVelocity.y = downwardForce;
                }
                else 
                    upwardVelocity.y = -0.2f;
            }
        }

        private bool OnSlope()
        {
            var onSlope = false;

            if (isSphereGrounded)
            {
                var maxDistance = _characterController.height + _characterController.radius + raySize;
                Debug.DrawRay(transform.position, Vector3.down, Color.red, 0.05f, false);
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,
                        maxDistance,
                        _characterLayerValue))
                    if (hit.normal != Vector3.up)
                        onSlope = true;
            }

            return onSlope;
        }

        private void UpdateSpeed()
        {
            currentSpeed = currentMovementType switch
            {
                MovementTypes.Stand => 0f,
                MovementTypes.Crouch => CrouchSpeed,
                MovementTypes.Walk => WalkSpeed,
                MovementTypes.Sprint => SprintSpeed,
                _ => throw new ArgumentOutOfRangeException()
            };

            currentSpeed *= GetMultiplier();
        }

        private float GetMultiplier()
        {
            var mult = baseSpeedMult;

            if (isBushMult) mult *= BushMult;
            if (isWerewolfMult) mult *= WerewolfMult;
            mult *= currentMultCoroutine;

            return mult;
        }

        public void UpdateHitbox()
        {
            var isWerewolf = _playerAnimation.isWerewolfEnabled;

            // Chooses the new character controller height
            float desiredHitboxHeight;
            float desiredCameraHeight;
            float desiredRenderShift;

            if (currentMovementType == MovementTypes.Crouch)
            {
                desiredHitboxHeight = _crouchedHitboxHeight;
                desiredCameraHeight = _crouchedCameraHeight;
                desiredRenderShift = _crouchedShiftVillager;
            }
            else
            {
                if (!isWerewolf)
                {
                    desiredHitboxHeight = _standingHitboxHeight;
                    desiredCameraHeight = _standingCameraHeightVillager;
                    desiredRenderShift = _standingShiftVillager;
                }
                else
                {
                    desiredHitboxHeight = _standingHitboxHeight;
                    desiredCameraHeight = _standingCameraHeightWerewolf;
                    desiredRenderShift = _shiftWerewolf;
                }
            }

            // Character controller modifier
            var smoothTime = Time.deltaTime * crouchSmoothTime;
            
            _characterController.height = Mathf.Lerp(_characterController.height, desiredHitboxHeight, smoothTime);
            // _characterController.height -= _characterController.skinWidth;
            Vector3 characterControllerCenter = _characterController.center;
            characterControllerCenter.y = _characterController.height * 0.5f;
            _characterController.center = characterControllerCenter;

            // Camera height modifier
            Vector3 localPosition = cameraHolder.transform.localPosition;
            localPosition.y = Mathf.Lerp(localPosition.y, desiredCameraHeight, smoothTime);
            cameraHolder.transform.localPosition = localPosition;
            
            // Camera render depth modifier
            _playerController.MoveRender(-desiredRenderShift,
                isWerewolf ? _playerController.WerewolfRender : _playerController.VillagerRender,
                smoothTime);
            
            // HeadBob
            _playerLook.HeadBob();
            
            // FOV Change according to movement
            _playerLook.FOVChanger();
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

        public void SetGroundedState(bool _grounded)
        {
            grounded = _grounded;
        }

        #endregion

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
            var refMult = baseSpeedMult;

            // First value
            while (timer <= duration * startTime)
            {
                var divider = duration * startTime;
                var progress = divider == 0 ? 1 : timer / divider;

                timer += Time.deltaTime;
                refMult = Mathf.Lerp(baseSpeedMult, targetValue, progress);

                // Apply it
                currentMultCoroutine = baseSpeedMult * refMult;
                yield return null;
            }

            // Waits for endTime
            while (timer < duration * endTime)
            {
                timer += Time.deltaTime;

                yield return null;
            }

            // Second value
            var finalMult = baseSpeedMult;
            while (timer < duration)
            {
                var progress = (timer - duration * endTime) / (duration * (1 - endTime));

                timer += Time.deltaTime;
                refMult = Mathf.Lerp(refMult, finalMult, progress);

                // Apply it
                currentMultCoroutine = baseSpeedMult * refMult;
                yield return null;
            }

            currentMultCoroutine = baseSpeedMult;
        }
    }
}