using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MainGame.PlayerScripts
{
    public class PlayerMovement : MonoBehaviour
    {
        #region Attributes
    
        // External GameObjects and components
        [SerializeField] private GameObject cameraHolder;
    
        // Player Controller & Controls
        private PlayerInput _playerInput;

        // Movement components
        private CharacterController _characterController;

        // private PlayerControls _playerControls;
        private bool WantsCrouchHold { get; set; }
        private bool _wantsCrouchToggle;
        private bool WantsSprint { get; set; }
        private bool WantsJump { get; set; }

        // Movement speeds
        [Space]
        [Header("Player speed settings")]
        [SerializeField] private float currentSpeed;
        public float currentSpeedMult = 1;
        private const float baseSpeedMult = 1;
        private const float SprintSpeed = 5f;
        private const float CrouchSpeed = 1f;
        private const float WalkSpeed = 2f;
        private const float smoothMoveValue = 10f;
    
        [Space]
        [Header("Movement settings")]
        [SerializeField] public MovementTypes currentMovementType = MovementTypes.Stand;
        [SerializeField] public CrouchModes currentCrouchType = CrouchModes.Hold;
        public Vector3 localMoveAmountNormalized;
        public Vector3 localMoveAmountRaw;
        private Vector3 inputMoveRaw3D;

        // Gravity
        [Space] [Header("Gravity settings")]
        private const float gravityForce = -9.81f;
        public Vector3 upwardVelocity = Vector3.zero;
    
        // Ground check
        [SerializeField] public float RaySize;
        public bool grounded;
        public float slopeCompensationForce = 5f;

        // Crouch & Hitboxes 
        [Space]
        [Header("Player height settings")]
        [SerializeField] private float crouchSmoothTime = 0f;
        private float _standingHitboxHeight = 1.8f;
        private float _crouchedHitboxHeight = 1.404f;
        private float _standingCameraHeight = 1.68f;
        private float _crouchedCameraHeight = 1.176f;
    
        // Jump values
        [Space]
        [Header("Player jump settings")]
        private const float JumpForce = 1f;
        private const float jumpCompensation = 1;
        public bool isJumping;

        public enum MovementTypes
        {
            Stand,
            Crouch,
            Walk,
            Sprint
        };
        public enum CrouchModes
        {
            Toggle,
            Hold
        };
    
        #endregion

        #region Unity methods

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();

            _characterController = GetComponentInChildren<CharacterController>();

            RaySize = _characterController.radius * 1.1f;
        
            float hitboxHeight = _characterController.height;
            _crouchedHitboxHeight = hitboxHeight * 0.78f;
            _standingHitboxHeight = hitboxHeight;

            float camHeight = cameraHolder.transform.localPosition.y;
            _crouchedCameraHeight = camHeight * 0.7f;
            _standingCameraHeight = camHeight;
        }

        private void Start()
        {
            // for the ZQSD movements
            _playerInput.actions["Move"].performed += OnPerformedMove;
            _playerInput.actions["Move"].canceled += _ => inputMoveRaw3D = Vector3.zero;
            // for the Crouch button
            _playerInput.actions["Crouch"].performed += ctx => WantsCrouchHold = ctx.ReadValueAsButton();
            _playerInput.actions["Crouch"].canceled += ctx => WantsCrouchHold = ctx.ReadValueAsButton();
        
            //TODO fix toggle
            _playerInput.actions["Crouch"].started += ctx => _wantsCrouchToggle = ctx.ReadValueAsButton();
            // for the Sprint button
            _playerInput.actions["Sprint"].performed += ctx => WantsSprint = ctx.ReadValueAsButton();
            _playerInput.actions["Sprint"].canceled += ctx => WantsSprint = ctx.ReadValueAsButton();
            // for the Jump button
            _playerInput.actions["Jump"].performed += ctx => WantsJump = ctx.ReadValueAsButton();
            _playerInput.actions["Jump"].canceled += ctx => WantsJump = ctx.ReadValueAsButton();
        }

        private void OnPerformedMove(InputAction.CallbackContext ctx)
        {
            inputMoveRaw3D = new Vector3
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
                x = inputMoveRaw3D.x,
                y = 0.0f,
                z = inputMoveRaw3D.z
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
            localMoveAmountNormalized = SmoothMoveAmount(localMoveAmountNormalized ,inputMoveNormalized3D);
            localMoveAmountRaw = SmoothMoveAmount(localMoveAmountRaw, inputMoveRaw3D);

            // Applies direction
            Vector3 currentMotion = transform.TransformDirection(localMoveAmountNormalized);
            currentMotion += upwardVelocity;
        
            currentMotion *= chosenDeltaTime;

            // Move
            _characterController.Move(currentMotion);
        }

        private void UpdateGrounded()
        {
            SetGroundedState(_characterController.isGrounded);
            // grounded = Physics.CheckBox(groundCheck.position, new Vector3(radius, 0.01f, radius / 2), Quaternion.identity);
            // grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        private void UpdateMovementState()
        {
            if (WantsCrouchHold)
            {
                currentMovementType = MovementTypes.Crouch;
            }
            else if (Vector3.zero == inputMoveRaw3D)
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
            upwardVelocity.y += gravityForce * Time.deltaTime;

            if (grounded && !isJumping)
            {
                if (OnSlope())
                {
                    float downwardForce = -GetAngleFromFloor() * slopeCompensationForce;
                    downwardForce = Mathf.Clamp(downwardForce, -2f, -500f);
                    
                    upwardVelocity.y = downwardForce;
                }
                else
                {
                    upwardVelocity.y = -2.0f;
                }
            }
        }

        private bool OnSlope()
        {
            if (!grounded) return false;

            bool onSlope = false;
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, RaySize))
            {
                if (hit.normal != Vector3.up)
                {
                    onSlope = true;
                }
            }
        
            return onSlope;
        }

        private float GetAngleFromFloor()
        {
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,
                _characterController.height / 2 * RaySize);
            float angle = Vector3.Angle(Vector3.up, hit.normal);

            return angle;
        }
    
        private void UpdateSpeed()
        {
            currentSpeed = GetSpeed();
        }

        private float GetSpeed()
        {
            var speed = currentMovementType switch
            {
                MovementTypes.Stand => 0f,
                MovementTypes.Crouch => CrouchSpeed,
                MovementTypes.Walk => WalkSpeed,
                MovementTypes.Sprint => SprintSpeed,
                _ => throw new ArgumentOutOfRangeException()
            };

            speed *= currentSpeedMult;
            
            return speed;
        }
    
        public void UpdateJump()
        {
            if (isJumping && grounded)
            {
                isJumping = false;
            }
        
            if (WantsJump && grounded)
            {
                upwardVelocity.y = Mathf.Sqrt(JumpForce * gravityForce * jumpCompensation * -2f);
                isJumping = true;
            }
        }

        public void UpdateHitbox()
        {
            // Chooses the new height
            float desiredHitboxHeight = currentMovementType switch
            {
                MovementTypes.Crouch => _crouchedHitboxHeight,
                _ => _standingHitboxHeight
            };

            AdjustPlayerHeight(desiredHitboxHeight);

            Vector3 camPosition = cameraHolder.transform.localPosition;

            camPosition.y = _crouchedCameraHeight + (_standingCameraHeight - _crouchedCameraHeight) *
                ((_characterController.height - _crouchedHitboxHeight) / (_standingHitboxHeight - _crouchedHitboxHeight));

            cameraHolder.transform.localPosition = camPosition;
        }
    
        // Sets the hitbox height to the input value progressively
        private void AdjustPlayerHeight(float desiredHeight)
        {
            _characterController.height = Mathf.Lerp(_characterController.height, desiredHeight, crouchSmoothTime);
            
            Vector3 characterControllerCenter = _characterController.center;
            characterControllerCenter.y = _characterController.height / 2f;
            
            _characterController.center = characterControllerCenter;
        }

    
        #endregion
    
        #region Setters

        private Vector3 SmoothMoveAmount(Vector3 localMoveAmount, Vector3 moveDir)
        {
            return Vector3.Lerp(localMoveAmount, moveDir * currentSpeed, Time.deltaTime * smoothMoveValue);
            // return Vector3.SmoothDamp(localMoveAmount, moveDir * currentSpeed, ref _, smoothTime);
        }
    
        public void SetGroundedState(bool _grounded)
        {
            grounded = _grounded;
        }

        #endregion

        public void StartSlowSpeed(float duration, float targetValue, float startTime, float endTime)
        {
            startTime = Mathf.Clamp01(startTime);
            endTime = Mathf.Clamp(endTime, startTime, 1);
            targetValue = Mathf.Clamp01(targetValue);
            
            StartCoroutine(SlowSpeed(duration, targetValue, startTime, endTime));
        }
        
        private IEnumerator SlowSpeed(float duration, float targetValue, float startTime, float endTime)
        {
            float timer = 0;
            var lastCurrentMult = currentSpeedMult;

            // Decreases
            while (timer < duration * startTime)
            {
                var progress = timer / (duration * startTime);
                
                timer += Time.deltaTime;
                currentSpeedMult = Mathf.Lerp(lastCurrentMult, targetValue, progress);

                yield return null;
            }
            
            // Waits for endTime
            while (timer < duration * endTime)
            {
                timer += Time.deltaTime;

                yield return null;
            }
            
            // Increases back
            lastCurrentMult = currentSpeedMult;
            while (timer < duration)
            {
                var progress = (timer - duration * endTime) / (duration * (1 - endTime));
                
                timer += Time.deltaTime;
                currentSpeedMult = Mathf.Lerp(lastCurrentMult, baseSpeedMult, progress);

                yield return null;
            }

            currentSpeedMult = baseSpeedMult;
        }
    }
}
