using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    #region Attributes
    
    // External GameObjects and components
    [SerializeField] private GameObject cameraHolder;
    
    // Player Controller & Controls
    private PlayerController _playerController;
    private PlayerAnimation _playerAnimation;
    private PlayerControls _playerControls;
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

    private const float SprintSpeed = 4f;
    private const float CrouchSpeed = 1f;
    private const float WalkSpeed = 2f;
    private const float SmoothTimeSpeedTransition = 0.5f;

    [Space]
    [Header("Movement settings")]
    [SerializeField] public MovementTypes currentMovementType = MovementTypes.Stand;
    [SerializeField] public CrouchModes currentCrouchType = CrouchModes.Hold;
    private Vector3 _;
    public Vector3 localMoveAmount;
    public Vector3 moveAmountRaw;
    private Vector3 _transformDirection;
    private Vector2 _inputMoveNormalized2D;
    private Vector3 _inputMoveNormalized3D;

    // Gravity
    [Space] [Header("Gravity settings")]
    private float gravityForce = -9.81f;
    public Vector3 upwardVelocity = Vector3.zero;
    
    // Ground check
    public bool grounded;

    public float floorDistanceMult = 1.2f;
    public float slopeCompensationForce = 60f;
    /*[SerializeField] public LayerMask groundMask;
    public Transform groundCheck;
    public float groundDistance = 0.01f;*/

    // Crouch & Hitboxes 
    [Space]
    [Header("Player height settings")]
    [SerializeField] private float crouchSmoothTime = 0.1f;
    private Vector3 _crouchSmoothVelocityVector3;
    private float _crouchSmoothVelocity;
    private float _standingHitboxHeight = 1.8f;
    private float _crouchedHitboxHeight = 1.404f;
    private float _standingCameraHeight = 1.68f;
    private float _crouchedCameraHeight = 1.176f;
    
    // Jump values
    [Space]
    [Header("Player jump settings")]
    [SerializeField] private float smoothTime = 0.10f; // Default 0.15: feel free to set back to default if needed
    private const float JumpForce = 2f;
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
        _playerController = GetComponent<PlayerController>();
        _playerAnimation = GetComponentInChildren<PlayerAnimation>();
        _playerInput = GetComponent<PlayerInput>();

        _characterController = GetComponentInChildren<CharacterController>();
        
        float hitboxHeight = _characterController.height;
        _crouchedHitboxHeight = hitboxHeight * 0.78f;
        _standingHitboxHeight = hitboxHeight;

        float camHeight = cameraHolder.transform.localPosition.y;
        _crouchedCameraHeight = camHeight * 0.7f;
        _standingCameraHeight = camHeight;
    }

    private void Start()
    {
        _playerControls = _playerController.PlayerControls;
        
        // for the ZQSD movements
        _playerInput.actions["Move"].performed += ctx => _inputMoveNormalized2D = ctx.ReadValue<Vector2>();
        _playerInput.actions["Move"].canceled += _ => _inputMoveNormalized2D = Vector2.zero;
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

    #endregion
    
    #region Movements

    public void Move()
    {
        _inputMoveNormalized3D = new Vector3
        {
            x = _inputMoveNormalized2D.x,
            y = 0.0f,
            z = _inputMoveNormalized2D.y
        };

        // Updates the grounded boolean state
        UpdateGrounded();

        // Updates the current movement type
        UpdateMovementState();

        // Updates gravity
        UpdateGravity(); // changes 'transformGravity'

        // Updates the speed based on the MovementType
        UpdateSpeed();

        // Sets the new movement vector based on the inputs
        localMoveAmount = SmoothMoveAmount(_inputMoveNormalized3D);
        
        

        Vector3 finalDirection = transform.TransformDirection(localMoveAmount) + upwardVelocity;
        finalDirection *= Time.fixedDeltaTime;

        _characterController.Move(finalDirection);
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
        else if (Vector2.zero == _inputMoveNormalized2D)
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
        upwardVelocity.y += gravityForce * Time.fixedDeltaTime;

        if (grounded && upwardVelocity.y < 0 && !OnSlope())
        {
            var unused = 0f;
            upwardVelocity.y = Mathf.SmoothDamp(upwardVelocity.y, -2.0f, ref unused, Time.fixedDeltaTime);
        }
        
        if (!isJumping && OnSlope())
        {
            upwardVelocity.y -= slopeCompensationForce * Time.deltaTime * _characterController.height / 2;
        }
    }

    private bool OnSlope()
    {
        return !grounded &&
             Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,
                 _characterController.height / 2 * floorDistanceMult) &&
              hit.normal != Vector3.up;
    }
    
    private void UpdateSpeed()
    {
        SetCurrentSpeed(GetSpeed());
    }

    private float GetMovementTransitionSpeed()
    {
        float res = Time.deltaTime;
        
        switch (currentMovementType)
        {
            case MovementTypes.Stand:
                 res *= 1f;
                break;
            case MovementTypes.Crouch:
                res *= 1.5f;
                break;
            case MovementTypes.Walk:
                res *= 1.2f;
                break;
            case MovementTypes.Sprint:
                res *= 2.5f;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        Debug.Log("transition speed:" + res);

        if (res < 0.1f) res = 0.1f;

        return res;
    }
    
    public float GetSpeed()
    {
        return currentMovementType switch
        {
            MovementTypes.Stand => 0f,
            MovementTypes.Crouch => CrouchSpeed,
            MovementTypes.Walk => WalkSpeed,
            MovementTypes.Sprint => SprintSpeed,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public void UpdateJump() // changes 'transformJump'
    {
        if (isJumping && grounded)
        {
            isJumping = false;
        }
        
        if (WantsJump && grounded)
        {
            upwardVelocity.y = Mathf.Sqrt(JumpForce * gravityForce * -2f);
            isJumping = true;
        }
    }

    public void UpdateHitbox()
    {
        #region Crouch management
        
        // This is for the CharacterController's dimensions and the camera position

        float desiredHitboxHeight = currentMovementType switch
        {
            MovementTypes.Crouch => _crouchedHitboxHeight,
            _ => _standingHitboxHeight
        };

        if (Math.Abs(_characterController.height - desiredHitboxHeight) > 0)
        {
            AdjustPlayerHeight(desiredHitboxHeight);
                
            Vector3 camPosition = cameraHolder.transform.localPosition;

            camPosition.y = _crouchedCameraHeight + (_standingCameraHeight - _crouchedCameraHeight) * ((_characterController.height - _crouchedHitboxHeight) / (_standingHitboxHeight - _crouchedHitboxHeight));
            
            cameraHolder.transform.localPosition = camPosition;
        }


        #endregion
    }
    
    // Sets the hitbox height to the input value progressively
    private void AdjustPlayerHeight(float desiredHeight)
    {
        // Calculates the current center of the player
        float center = desiredHeight / 2f;

        _characterController.height = Mathf.SmoothDamp(_characterController.height, desiredHeight, ref _crouchSmoothVelocity, crouchSmoothTime);
        _characterController.center = Vector3.SmoothDamp(_characterController.center, new Vector3(0, center, 0), ref _crouchSmoothVelocityVector3, crouchSmoothTime);
    }

    
    #endregion
    
    #region Setters

    private Vector3 SmoothMoveAmount(Vector3 moveDir)
    {
        return Vector3.SmoothDamp(localMoveAmount, moveDir * currentSpeed, ref _,
            smoothTime);
    }
    
    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    private void SetCurrentMovementType(MovementTypes _currentMovementType)
    {
        currentMovementType = _currentMovementType;
    }

    private void SetCurrentSpeed(float targetSpeed)
    {
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, SmoothTimeSpeedTransition);
    }
    
    #endregion
}
