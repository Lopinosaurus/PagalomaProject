using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerController)), RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    #region Attributes
    
    // External GameObects and components
    [SerializeField] private GameObject cameraHolder = null;

    // Movement components
    private CharacterController _characterController;
    
    // Network component
    private PhotonView _photonView;
    
    // Subscripts
    private PlayerController _playerController;
    
    
    // Movement speeds
    [Space]
    [Header("Player speed settings")]
    [SerializeField] private float sprintSpeed = 4f;
    private float walkSpeed = 2f;
    private float crouchSpeed = 1f;
    [SerializeField] private float currentSpeed;
    
    // Gravity
    [Space] [Header("Gravity settings")]
    [SerializeField] private float gravityForce = -9.81f;
    [SerializeField] private Vector3 velocity = Vector3.zero;
    
    // Ground check
    public Transform groundCheck;
    [SerializeField] public LayerMask groundMask;
    public float groundDistance = 0.4f;
    public bool grounded;

    // Heights
    [Space]
    [Header("Player height settings")]
    private float crouchedHeight;
    private float standingHeight;
    
    // Jump values
    [Space]
    [Header("Player jump settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float smoothTime = 0.10f; // Default 0.15: feel free to set back to default if needed
    
    private Vector3 smoothMoveVelocity;
    public Vector3 moveAmount = Vector3.zero;
    
    [Space]
    [Header("Movement settings")]
    [SerializeField] public MovementTypes currentMovementType = MovementTypes.Stand;
    [SerializeField] public CrouchModes currentCrouchType = CrouchModes.Hold;

    [SerializeField] public enum MovementTypes
    {
        Stand,
        Crouch,
        Walk,
        Sprint
    };
    [SerializeField] public enum CrouchModes
    {
        Toggle,
        Hold
    };
    
    #endregion

    #region Unity methods

    private void Awake() // Don't touch !
    {
        _photonView = GetComponent<PhotonView>();
        _playerController = GetComponent<PlayerController>();
        _characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        var height = _characterController.height;
        crouchedHeight = height / 2;
        standingHeight = height;
    }

    #endregion
    
    public void UpdateHitbox(MovementTypes _currentMovementType)
    {
        // Updates player's hitbox about crouch
        #region Crouch management
        
        float desiredHeight = standingHeight;
        if (MovementTypes.Crouch == currentMovementType) desiredHeight = crouchedHeight;

        Vector3 camPosition = cameraHolder.transform.localPosition;
        if (Math.Abs(_characterController.height - desiredHeight) > 0)
        {
            AdjustHeight(desiredHeight);
            camPosition.y = _characterController.height;
        }
        
        cameraHolder.transform.localPosition = camPosition;

        #endregion
    }
    
    
    // Sets the hitbox's height to the input value progressively
    private void AdjustHeight(float height)
    {
        // Calculates the current center of the player
        float center = height / 2f;

        _characterController.height = Mathf.Lerp(_characterController.height, height, crouchSpeed);
        _characterController.center = Vector3.Lerp(_characterController.center, new Vector3(0, center, 0), crouchSpeed);
    }

    #region Movements
    
    public void Move()
    {
        Vector3 moveDir = new Vector3
        {
            x = Input.GetAxisRaw("Horizontal"),
            z = Input.GetAxisRaw("Vertical")
        };
        moveDir = moveDir.normalized;
        
        // Updates the grounded boolean state
        UpdateGrounded();

        // Updates the sprinting state
        UpdateSprint();

        // Updates the crouching state
        UpdateCrouch();

        // Updates the walk state
        UpdateWalk(moveDir);

        // Updates gravity
        UpdateGravity(); // changes 'transformGravity'

        // Updates the speed based on the MovementType
        UpdateSpeed();
        
        // Sets the new movement vector based on the inputs
        SetMoveAmount(moveDir);
        
        

        if (Vector3.zero != velocity)
        {
            // Debug.Log("velocity vector is: " + velocity);
        }
        
        // Applies direction from directional inputs
        Vector3 transformDirection = transform.TransformDirection(moveAmount);
        
        _characterController.Move( transformDirection * Time.fixedDeltaTime);
        _characterController.Move(velocity * Time.fixedDeltaTime);
        // _rigidBody.MovePosition(_rigidBody.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }
    
    private bool UpdateSprint()
    {
        if (MovementTypes.Crouch == currentMovementType) return false;

        // Checks if players wants to sprint and if he pressed a directional key
        if (Input.GetButton("Sprint") && (0 != Input.GetAxisRaw("Horizontal") || 0 != Input.GetAxisRaw("Vertical")))
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
            case CrouchModes.Hold when Input.GetButton("Crouch"):

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
                if (Input.GetButtonDown("Crouch"))
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

    public bool UpdateJump() // changes 'transformJump'
    {
        // _characterController.AddForce(transform.up * jumpForce);

        if (Input.GetButtonDown("Jump"))
        {
            Debug.Log("input fo jump detected");
        }

        if (Input.GetButtonDown("Jump") && grounded)
        {
            Debug.Log("Should jump");
            velocity.y = Mathf.Sqrt(jumpForce * gravityForce * -2f);
        }

        return true; // for now, no conditions prevents the player from jumping
    }
    
    private void UpdateGravity()
    {
        velocity.y += gravityForce * Time.fixedDeltaTime;

        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f;
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

    private void UpdateGrounded()
    {
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    #endregion
    
    #region Setters

    private void SetMoveAmount(Vector3 moveDir)
    {
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * currentSpeed, ref smoothMoveVelocity, smoothTime);
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
