using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Photon.Pun;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerController))]

public class PlayerMovement : MonoBehaviour
{
    #region Attributes
    
    // External GameObjects and components
    [SerializeField] private GameObject cameraHolder;
    
    // Player Controller & Controls
    private PlayerController _playerController;
    private PlayerControls _playerControls;

    // Movement components
    private CharacterController _characterController;

    // private PlayerControls _playerControls;
    private Vector2 _moveRaw2D;
    private bool _wantsCrouchHold;
    private bool _wantsCrouchToggle;
    private bool _wantsSprint;
    private bool _wantsJump;

    // Movement speeds
    [Space]
    [Header("Player speed settings")]
    [SerializeField] private float currentSpeed;
    private const float SprintSpeed = 6f;
    private const float CrouchSpeed = 2f;
    private const float WalkSpeed = 4f;
    private const float SmoothTimeSpeedTransition = 0.5f;

    [Space]
    [Header("Movement settings")]
    [SerializeField] public MovementTypes currentMovementType = MovementTypes.Stand;
    [SerializeField] public CrouchModes currentCrouchType = CrouchModes.Hold;
    private Vector3 _moveSmoothVelocity;
    public Vector3 moveAmount = Vector3.zero;

    // Gravity
    [Space] [Header("Gravity settings")]
    private float gravityForce = -9.81f;
    private Vector3 _velocity = Vector3.zero;
    
    // Ground check
    public bool grounded;
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
        // Player Controller
        _playerController = GetComponent<PlayerController>();

        _characterController = GetComponent<CharacterController>();
        
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
        _playerControls.Player.Enable();

        // for the ZQSD movements
        _playerControls.Player.Move.performed += ctx => _moveRaw2D = ctx.ReadValue<Vector2>();
        _playerControls.Player.Move.canceled += _ => _moveRaw2D = Vector2.zero;
        // for the Crouch button
        _playerControls.Player.Crouch.performed += ctx => _wantsCrouchHold = ctx.ReadValueAsButton();
        _playerControls.Player.Crouch.canceled += ctx => _wantsCrouchHold = ctx.ReadValueAsButton();
        
        //TODO fix toggle
        _playerControls.Player.Crouch.started += ctx => _wantsCrouchToggle = ctx.ReadValueAsButton();
        // for the Sprint button
        _playerControls.Player.Sprint.performed += ctx => _wantsSprint = ctx.ReadValueAsButton();
        _playerControls.Player.Sprint.canceled += ctx => _wantsSprint = ctx.ReadValueAsButton();
        // for the Jump button
        _playerControls.Player.Jump.performed += ctx => _wantsJump = ctx.ReadValueAsButton();
        _playerControls.Player.Jump.canceled += ctx => _wantsJump = ctx.ReadValueAsButton();
    }

    #endregion
    
    #region Movements

    public void Move()
    {
        Vector3 moveRaw3D = new Vector3
        {
            x = _moveRaw2D.x,
            y = 0.0f,
            z = _moveRaw2D.y
        };
        
        moveRaw3D = moveRaw3D.normalized;

        // Updates the grounded boolean state
        UpdateGrounded();

        // Updates the sprinting state
        UpdateSprint();

        // Updates the crouching state
        UpdateCrouch();

        // Updates the walk state
        UpdateWalk();

        // Updates gravity
        UpdateGravity(); // changes 'transformGravity'

        // Updates the speed based on the MovementType
        UpdateSpeed();
        
        // Sets the new movement vector based on the inputs
        SetMoveAmount(moveRaw3D); // changes 'moveAmount'
        
        
        // Applies direction from directional inputs
        Vector3 transformDirection = transform.TransformDirection(moveAmount);
        
        _characterController.Move( transformDirection * Time.fixedDeltaTime);
        _characterController.Move(_velocity * Time.fixedDeltaTime);
        // _rigidBody.MovePosition(_rigidBody.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }
    
    private void UpdateGrounded()
    {
        SetGroundedState(_characterController.isGrounded);
        // grounded = Physics.CheckBox(groundCheck.position, new Vector3(radius, 0.01f, radius / 2), Quaternion.identity);
        // grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    private void UpdateSprint()
    {
        if (MovementTypes.Crouch == currentMovementType) return;

        // Checks if players wants to sprint and if he pressed a directional key
        if (_wantsSprint && Vector2.zero != _moveRaw2D)
        {
            SetCurrentMovementType(MovementTypes.Sprint);
        }
        else
        {
            SetCurrentMovementType(MovementTypes.Stand);
        }
    }
    
    private void UpdateCrouch()
    {
        switch (currentCrouchType)
        {
            case CrouchModes.Hold:
                if (_wantsCrouchHold)
                    SetCurrentMovementType(MovementTypes.Crouch);
                else if (MovementTypes.Crouch == currentMovementType)
                    SetCurrentMovementType(MovementTypes.Stand);
                return;
                
            case CrouchModes.Toggle:
            {
                if (_wantsCrouchToggle)
                {
                    SetCurrentMovementType(MovementTypes.Crouch == currentMovementType
                        ? MovementTypes.Stand
                        : MovementTypes.Crouch);
                }
            
                return;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void UpdateWalk()
    {
        if (MovementTypes.Stand == currentMovementType && Vector2.zero != _moveRaw2D)
        {
            SetCurrentMovementType(MovementTypes.Walk);
        }
    }
    
    private void UpdateGravity()
    {
        _velocity.y += gravityForce * Time.fixedDeltaTime;

        if (grounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        if (!grounded && _velocity.y < 0)
        {
            //TODO make the player stick to the ground
        }
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
    
    private float GetSpeed()
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
    
    public bool UpdateJump() // changes 'transformJump'
    {
        if (_wantsJump && grounded)
        {
            _velocity.y = Mathf.Sqrt(JumpForce * gravityForce * -2f);
        }

        return true; // for now, no conditions prevents the player from jumping
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

    private void SetMoveAmount(Vector3 moveDir)
    {
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * currentSpeed, ref _moveSmoothVelocity,
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
