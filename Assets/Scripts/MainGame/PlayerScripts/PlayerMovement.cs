using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Photon.Pun;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerController))]

public class PlayerMovement : MonoBehaviour
{
    #region Attributes
    
    // External GameObjects and components
    [SerializeField] private GameObject cameraHolder;

    // Movement components
    private CharacterController _characterController;
    
    // Player Controls
    private PlayerController _playerController;
    private static PlayerControls _playerControls;

    // private PlayerControls _playerControls;
    private Vector2 moveRaw2D;
    private bool wantsCrouch;
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
    public Transform groundCheck;
    [SerializeField] public LayerMask groundMask;
    public float groundDistance = 0.01f;
    public bool grounded;

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

    private void Awake() // Don't touch !
    {
        // Player Controls
        _playerControls = new PlayerControls();

        _characterController = GetComponent<CharacterController>();
        _playerController = GetComponent<PlayerController>();
        
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
        _playerControls.Player.Crouch.performed += ctx => wantsCrouch = ctx.ReadValueAsButton();
        _playerControls.Player.Crouch.canceled += ctx => wantsCrouch = ctx.ReadValueAsButton();
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
        UpdateWalk(moveRaw3D);

        // Updates gravity
        UpdateGravity(); // changes 'transformGravity'

        // Updates the speed based on the MovementType
        UpdateSpeed();
        
        // Sets the new movement vector based on the inputs
        SetMoveAmount(moveRaw3D);


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

    private bool UpdateSprint()
    {
        if (MovementTypes.Crouch == currentMovementType) return false;

        // Checks if players wants to sprint and if he pressed a directional key
        if (wantsSprint && Vector2.zero != moveRaw2D)
        {
            return SetCurrentMovementType(MovementTypes.Sprint);
        }
        else
        {
            return SetCurrentMovementType(MovementTypes.Stand);
        }
    }
    
    private bool UpdateCrouch()
    {
        // TODO
        //  Simplify the crouch system including the Hold and Toggle options
        
        // Checks the current crouch mode (toggle or hold)
        switch (currentCrouchType)
        {
            // Will crouch the player
            case CrouchModes.Hold when wantsCrouch:

                // Sets the MovementType to crouched
                return SetCurrentMovementType(MovementTypes.Crouch);
            
            // Will uncrouch the player if there is no input
            case CrouchModes.Hold:
            {
                if (MovementTypes.Crouch == currentMovementType)
                {
                    // Sets the MovementType to stand
                    return SetCurrentMovementType(MovementTypes.Stand);
                }
                    
                break;
            }

            case CrouchModes.Toggle:
            {
                if (wantsCrouch)
                {
                    // Checks the current state of crouch
                    if (currentMovementType == MovementTypes.Crouch)
                    {
                        // Checks if UNcrouching is possible (example: no obstacles above)
                        if (true)
                        {
                            // Sets the MovementType to stand
                            return SetCurrentMovementType(MovementTypes.Stand);
                        }
                    }
                    else
                    {
                        // Sets the MovementType to crouched
                        return SetCurrentMovementType(MovementTypes.Crouch);
                    }
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException("A movement type other than .Stand and .Crouch was entered:" + currentCrouchType.ToString());
        }

        return false;
    }
    
    private bool UpdateWalk(Vector3 _moveDir)
    {
        if (MovementTypes.Stand == currentMovementType && Vector3.zero != _moveDir)
        {
            return SetCurrentMovementType(MovementTypes.Walk);
        }

        return false;
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
            //TODO
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
