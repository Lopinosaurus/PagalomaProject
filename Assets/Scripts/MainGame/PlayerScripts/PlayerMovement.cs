using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MainGame.PlayerScripts
{
    public partial class PlayerMovement : MonoBehaviour
    {
        #region Attributes
    
        // External GameObjects and components
        [SerializeField] private GameObject cameraHolder;
    
        // Player Controller & Controls
        private PlayerInput _playerInput;
        private PlayerController _playerController;

        // Movement components
        [HideInInspector] public CharacterController _characterController;

        private bool WantsCrouchHold { get; set; }
        private bool WantsSprint { get; set; }

        // Movement speeds
        [Space]
        [Header("Player speed settings")]
        [SerializeField] private float currentSpeed;
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
        
        [Space]
        [Header("Movement settings")]
        [HideInInspector]
        [SerializeField] public MovementTypes currentMovementType = MovementTypes.Stand;

        public Vector3 localMoveAmountNormalized;
        private Vector3 _inputMoveRaw3D;

        // Gravity
        [Space] [Header("Gravity settings")]
        private const float GravityForce = -9.81f;
        public Vector3 upwardVelocity = Vector3.zero;
    
        // Ground check
        private float raySize = 0.1f;
        public bool grounded;
        private float slopeCompensationForce = 5f;
        public bool isSphereGrounded { get; set; }
        private bool isCCgrounded { get; set; }

        // Crouch & Hitboxes 
        [Space]
        [Header("Player height settings")]
        [SerializeField] private float crouchSmoothTime = 10;
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

            _characterController = GetComponentInChildren<CharacterController>();
            _characterLayerValue = (int)Mathf.Log(characterLayer.value, 2);
            
            raySize = _characterController.radius * 1.1f;
        
            // Hitboxes
            float hitboxHeight = _characterController.height;
            _standingHitboxHeight = hitboxHeight;
            _crouchedHitboxHeight = hitboxHeight * 0.7f;

            // Cam heights
            Vector3 camPos = cameraHolder.transform.localPosition;
            _standingCameraHeightVillager = camPos.y;
            _standingCameraHeightWerewolf = 1.6f;
            _crouchedCameraHeight = 0.8f;
            
            // Profs
            _standingShiftVillager = _playerController.backShift;
            _crouchedShiftVillager = 0.7f;
            _shiftWerewolf = 1.5f;
        }

        private void Start()
        {
            // For glitch cc abuse
            if (RoomManager.Instance != null)
            {
                StartCoroutine(IgnorePlayerCollisions());
            }            
            // for the ZQSD movements
            _playerInput.actions["Move"].performed += OnPerformedMove;
            _playerInput.actions["Move"].canceled += _ => _inputMoveRaw3D = Vector3.zero;
            // for the Crouch button
            _playerInput.actions["Crouch"].performed += ctx => WantsCrouchHold = ctx.ReadValueAsButton();
            _playerInput.actions["Crouch"].canceled += ctx => WantsCrouchHold = ctx.ReadValueAsButton();
        
            // for the Sprint button
            _playerInput.actions["Sprint"].performed += ctx => WantsSprint = ctx.ReadValueAsButton();
            _playerInput.actions["Sprint"].canceled += ctx => WantsSprint = ctx.ReadValueAsButton();
            // for the Jump button
            _playerInput.actions["Jump"].performed += ctx => WantsJump = ctx.ReadValueAsButton();
            _playerInput.actions["Jump"].canceled += ctx => WantsJump = ctx.ReadValueAsButton();
        }

        private IEnumerator IgnorePlayerCollisions()
        {
            yield return new WaitUntil(() => RoomManager.Instance.localPlayer != null);
            
            foreach (var role in RoomManager.Instance.players)
            {
                Physics.IgnoreCollision(_characterController,
                    role.gameObject.GetComponent<CharacterController>(), true);
            }
        }

        private void OnPerformedMove(InputAction.CallbackContext ctx)
        {
            _inputMoveRaw3D = new Vector3
            {
                x = ctx.ReadValue<Vector2>().x,
                z = ctx.ReadValue<Vector2>().y
            };
        }

        #endregion
    
        #region Movements

        public void Move(float chosenDeltaTime)
        {
            Vector3 inputMoveNormalized3D = new Vector3
            {
                x = _inputMoveRaw3D.x,
                y = 0.0f,
                z = _inputMoveRaw3D.z
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
            SmoothMoveAmount(inputMoveNormalized3D);
            
            // Applies direction
            Vector3 currentMotion = transform.TransformDirection(localMoveAmountNormalized);

            currentMotion += upwardVelocity;
            currentMotion *= chosenDeltaTime;
            
            // Removes moves if needed
            if (shouldJumpFreezeGravity) currentMotion *= 0;

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
            if (WantsCrouchHold && !_playerAnimation.isWerewolfEnabled)
            {
                currentMovementType = MovementTypes.Crouch;
            }
            else if (Vector3.zero == _inputMoveRaw3D)
            {
                currentMovementType = MovementTypes.Stand;
            }
            else if (WantsSprint)
            {
                currentMovementType = MovementTypes.Sprint;
            }
            else
            {
                currentMovementType = MovementTypes.Walk;
            }
        }

        private void UpdateGravity()
        {
            upwardVelocity.y += GravityForce * Time.deltaTime;

            if (grounded || shouldJumpFreezeGravity)
            {
                if (OnSlope() && !shouldJumpFreezeGravity)
                {
                    float downwardForce = -slopeCompensationForce;
                    downwardForce = Mathf.Clamp(downwardForce, -500, -2);
                    
                    upwardVelocity.y = downwardForce;
                }
                else
                {
                    upwardVelocity.y = -2;
                }
            }
        }

        private bool OnSlope()
        {
            bool onSlope = false;

            if (isSphereGrounded)
            {
                float maxDistance = _characterController.height + _characterController.radius + raySize;
                Debug.DrawRay(transform.position, Vector3.down, Color.red, 0.05f, false);
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,
                        maxDistance,
                        _characterLayerValue))
                {
                    if (hit.normal != Vector3.up) onSlope = true;
                }
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
            float mult = baseSpeedMult;

            if (isBushMult) mult *= BushMult;
            if (isWerewolfMult) mult *= WerewolfMult;
            mult *= currentMultCoroutine;
            
            return mult;
        }

        public void UpdateHitbox()
        {
            bool isWerewolf = _playerAnimation.isWerewolfEnabled;

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

            Debug.Log($"{desiredRenderShift}");
            
            // Character controller modifier
            float smoothTime = Time.deltaTime * crouchSmoothTime;
            
            _characterController.height = Mathf.Lerp(_characterController.height, desiredHitboxHeight, smoothTime);
            // _characterController.height -= _characterController.skinWidth;
            Vector3 characterControllerCenter = _characterController.center;
            characterControllerCenter.y = _characterController.height * 0.5f;
            _characterController.center = characterControllerCenter;
            
            // Camera height modifier
            var localPosition = cameraHolder.transform.localPosition;
            localPosition.y = Mathf.Lerp(localPosition.y, desiredCameraHeight, smoothTime);
            _playerController.MoveRender(-desiredRenderShift,
                isWerewolf ? _playerController.WerewolfRender : _playerController.VillagerRender,
                smoothTime);
            cameraHolder.transform.localPosition = localPosition;
        }
    
        #endregion
    
        #region Setters

        private void SmoothMoveAmount(Vector3 moveDir)
        {
            Vector3 raw = moveDir * currentSpeed;
            
            // Caps the speed to not run backwards too fast
            if (raw.z < -SprintBackSpeed) raw.z = -SprintBackSpeed;
            
            Vector3 amount = Vector3.Lerp(localMoveAmountNormalized, raw, Time.deltaTime * SmoothMoveValue);

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
            float refMult = baseSpeedMult;
            
            // First value
            while (timer <= duration * startTime)
            {
                float divider = duration * startTime;
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
            float finalMult = baseSpeedMult;
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
