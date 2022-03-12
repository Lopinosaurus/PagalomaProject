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
    private Vector2 moveRaw2D;
    private bool wantsCrouchHold;
    private bool wantsCrouchToggle;
    private bool wantsSprint;
    private bool wantsJump;

    // Movement speeds
    [Space]
    [Header("Player speed settings")]
    [SerializeField] private float currentSpeed;
    private const float sprintSpeed = 6f;
    private const float crouchSpeed = 2f;
    private const float walkSpeed = 4f;

    [Space]
    [Header("Movement settings")]
    [SerializeField] public MovementTypes currentMovementType = MovementTypes.Stand;
    [SerializeField] public CrouchModes currentCrouchType = CrouchModes.Hold;
    private Vector3 moveSmoothVelocity;
    public Vector3 moveAmount = Vector3.zero;

    // Gravity
    [Space] [Header("Gravity settings")]
    private float gravityForce = -9.81f;
    private Vector3 velocity = Vector3.zero;
    
    // Ground check
    public bool grounded;
    /*[SerializeField] public LayerMask groundMask;
    public Transform groundCheck;
    public float groundDistance = 0.01f;*/

    // Crouch & Hitboxes 
    [Space]
    [Header("Player height settings")]
    [SerializeField] private float crouchSmoothTime = 0.1f;
    private Vector3 crouchSmoothVelocityVector3;
    private float crouchSmoothVelocity;
    private float standingHitboxHeight = 1.8f;
    private float crouchedHitboxHeight = 1.404f;
    private float standingCameraHeight = 1.68f;
    private float crouchedCameraHeight = 1.176f;
    
    // Jump values
    [Space]
    [Header("Player jump settings")]
    [SerializeField] private float smoothTime = 0.10f; // Default 0.15: feel free to set back to default if needed
    private float jumpForce = 2f;

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
        _playerControls = _playerController.playerControls;

        _characterController = GetComponent<CharacterController>();
        
        float hitboxHeight = _characterController.height;
        crouchedHitboxHeight = hitboxHeight * 0.78f;
        standingHitboxHeight = hitboxHeight;

        float camHeight = cameraHolder.transform.localPosition.y;
        crouchedCameraHeight = camHeight * 0.7f;
        standingCameraHeight = camHeight;
    }

    private void OnEnable()
    {
        _playerControls.Player.Enable();
    }
    private void OnDisable()
    {    
        _playerControls.Player.Disable();
    }

    private void Start()
    {
        // for the ZQSD movements
        _playerControls.Player.Move.performed += ctx => moveRaw2D = ctx.ReadValue<Vector2>();
        _playerControls.Player.Move.canceled += _ => moveRaw2D = Vector2.zero;
        // for the Crouch button
        _playerControls.Player.Crouch.performed += ctx => wantsCrouchHold = ctx.ReadValueAsButton();
        _playerControls.Player.Crouch.canceled += ctx => wantsCrouchHold = ctx.ReadValueAsButton();
        _playerControls.Player.Crouch.started += ctx => wantsCrouchToggle = ctx.ReadValueAsButton();
        // for the Sprint button
        _playerControls.Player.Sprint.performed += ctx => wantsSprint = ctx.ReadValueAsButton();
        _playerControls.Player.Sprint.canceled += ctx => wantsSprint = ctx.ReadValueAsButton();
        // for the Jump button
        _playerControls.Player.Jump.performed += ctx => wantsJump = ctx.ReadValueAsButton();
        _playerControls.Player.Jump.canceled += ctx => wantsJump = ctx.ReadValueAsButton();
    }

    #endregion
    
    #region Movements

    public void Move()
    {
        Vector3 moveRaw3D = new Vector3
        {
            x = moveRaw2D.x,
            y = 0.0f,
            z = moveRaw2D.y
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
        _characterController.Move(velocity * Time.fixedDeltaTime);
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
        if (wantsSprint && Vector2.zero != moveRaw2D)
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
        if (MovementTypes.Sprint == currentMovementType) return;
        
        switch (currentCrouchType)
        {
            case CrouchModes.Hold:
                SetCurrentMovementType(wantsCrouchHold
                    ? MovementTypes.Crouch
                    : MovementTypes.Stand);
            
                return;
            case CrouchModes.Toggle:
            {
                if (wantsCrouchToggle)
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
        if (MovementTypes.Stand == currentMovementType && Vector2.zero != moveRaw2D)
        {
            SetCurrentMovementType(MovementTypes.Walk);
        }
    }
    
    private void UpdateGravity()
    {
        velocity.y += gravityForce * Time.fixedDeltaTime;

        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (!grounded && velocity.y < 0)
        {
            //TODO make the player stick to the ground
        }
    }
    
    private void UpdateSpeed()
    {
        switch (currentMovementType)
        {
            case MovementTypes.Stand:
                SetCurrentSpeed(0f);
                break;
            case MovementTypes.Crouch:
                SetCurrentSpeed(crouchSpeed);
                break;
            case MovementTypes.Walk:
                SetCurrentSpeed(walkSpeed);
                break;
            case MovementTypes.Sprint:
                SetCurrentSpeed(sprintSpeed);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public bool UpdateJump() // changes 'transformJump'
    {
        if (wantsJump && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * gravityForce * -2f);
        }

        return true; // for now, no conditions prevents the player from jumping
    }

    public void UpdateHitbox()
    {
        #region Crouch management
        
        // This is for the CharacterController's dimensions and the camera position

        float desiredHitboxHeight = currentMovementType switch
        {
            MovementTypes.Crouch => crouchedHitboxHeight,
            _ => standingHitboxHeight
        };

        if (Math.Abs(_characterController.height - desiredHitboxHeight) > 0)
        {
            AdjustPlayerHeight(desiredHitboxHeight);
                
            Vector3 camPosition = cameraHolder.transform.localPosition;

            camPosition.y = crouchedCameraHeight + (standingCameraHeight - crouchedCameraHeight) * ((_characterController.height - crouchedHitboxHeight) / (standingHitboxHeight - crouchedHitboxHeight));
            
            cameraHolder.transform.localPosition = camPosition;
        }


        #endregion
    }
    
    // Sets the hitbox's height to the input value progressively
    private void AdjustPlayerHeight(float desiredHeight)
    {
        // Calculates the current center of the player
        float center = desiredHeight / 2f;

        _characterController.height = Mathf.SmoothDamp(_characterController.height, desiredHeight, ref crouchSmoothVelocity, crouchSmoothTime);
        _characterController.center = Vector3.SmoothDamp(_characterController.center, new Vector3(0, center, 0), ref crouchSmoothVelocityVector3, crouchSmoothTime);
    }

    
    #endregion
    
    #region Setters

    private void SetMoveAmount(Vector3 moveDir)
    {
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * currentSpeed, ref moveSmoothVelocity, smoothTime);
    }
    
    public bool SetGroundedState(bool _grounded)
    {
        // Checks whether a change occured
        bool changeOccured = _grounded != grounded;
        
        grounded = _grounded;
        
        return changeOccured;
    }

    private bool SetCurrentMovementType (MovementTypes _currentMovementType)
    {
        // Checks whether a change occured
        bool changeOccured = _currentMovementType != currentMovementType;
        
        currentMovementType = _currentMovementType;
        
        return changeOccured;
    }

    private bool SetCurrentSpeed(float _currentSpeed)
    {
        // Checks whether a change occured
        bool changeOccured = Math.Abs(currentSpeed - _currentSpeed) > 0f;
        
        //TODO
        //Find a way to smoothen the speed transition
        currentSpeed = _currentSpeed;
        
        return changeOccured;
    }
    
    #endregion
}
