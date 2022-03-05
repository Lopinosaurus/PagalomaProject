using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
    
    
    // Movement speeds variables
    [Space]
    [Header("Player speed settings")]
    [SerializeField] private float sprintSpeed = 4f;
    private float walkSpeed = 2f;
    private float crouchSpeed = 1f;
    [SerializeField] private float currentSpeed;
<<<<<<< Updated upstream
=======
    private float sprintSpeed = 6f;
    private float crouchSpeed = 2f;
    private float walkSpeed = 4f;
    private Vector2 moveRaw2D;
    private Vector3 smoothMoveVelocity;
    private Vector3 moveAmount = Vector3.zero;
>>>>>>> Stashed changes
    
    // Gravity variables
    [Space] [Header("Gravity settings")]
    [SerializeField] private float gravityForce = -9.81f;
    [SerializeField] private Vector3 velocity = Vector3.zero;
    
    // Ground check variables
    public Transform groundCheck;
    [SerializeField] public LayerMask groundMask;
    public float groundDistance = 0.4f;
    public bool grounded;

    // Heights variables
    [Space]
    [Header("Player height settings")]
    private float crouchedHeight;
    private float standingHeight;
    
    // Jump variables
    [Space]
    [Header("Player jump settings")]
    private float jumpForce = 2f;
    [SerializeField] private float smoothTime = 0.10f; // Default 0.15: feel free to set back to default if needed
    
    // Crouch variables
    private bool wantsCrouch = false;

    [Space]
    [Header("Movement settings")]
    [SerializeField] public MovementTypes currentMovementType = MovementTypes.Stand;
    [SerializeField] public CrouchTypes currentCrouchType = CrouchTypes.Hold;

    [SerializeField] public enum MovementTypes
    {
        Stand,
        Crouch,
        Walk,
        Sprint
    };
<<<<<<< Updated upstream
    [SerializeField] public enum CrouchModes
=======
    public enum CrouchTypes
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
        
        // Player Controls
        _playerControls = new PlayerControls();
        
        // Movement inputs with ZQSD
        _playerControls.Player.Move.performed += ctx => moveRaw2D = ctx.ReadValue<Vector2>();
        _playerControls.Player.Move.canceled += _ => moveRaw2D = Vector2.zero;
        
        // Crouch inputs
        if (CrouchTypes.Hold == currentCrouchType)
        {
            wantsCrouch = _playerControls.Player.Crouch.WasPerformedThisFrame();

        }
        else
        {
            wantsCrouch = _playerControls.Player.Crouch.WasPressedThisFrame();
            wantsCrouch = _playerControls.Player.Crouch.WasReleasedThisFrame();
        }
    }
    
    private void OnEnable()
    {
        _playerControls.Player.Enable();
    }
    private void OnDisable()
    {    
        _playerControls.Player.Disable();
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
    
    public void Move()
    {
        Vector3 moveDir = new Vector3
        {
            x = Input.GetAxisRaw("Horizontal"),
            z = Input.GetAxisRaw("Vertical")
        };
        moveDir = moveDir.normalized;
        
=======

    private void OnMove(InputValue movementValue)
            {
                
            }
    
    public void Move()
    {
        /*InputValue movementValue = _playerControls.Player.Move.performed;
        Vector2 movementVector = movementValue.Get<Vector2>();
        
        float movementX = movementVector.x;
        float movementY = movementVector.y;*/
        
        // Creates a Vector3 based on inputs
        
        Vector3 moveRaw3D = new Vector3 {
            x = moveRaw2D.x,
            y = 0.0f,
            z = moveRaw2D.y
        }.normalized;

>>>>>>> Stashed changes
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
        SetMoveAmount(moveRaw3D); // modifies moveAmount
        
        

        // Converts local direction vector to world direction vector
        Vector3 transformDirection = transform.TransformDirection(moveAmount);
        
        Debug.Log("wants to crouch: " + wantsCrouch);
        
        _characterController.Move( transformDirection * Time.fixedDeltaTime);
        _characterController.Move(velocity * Time.fixedDeltaTime);
        // _rigidBody.MovePosition(_rigidBody.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }
    
    private bool UpdateSprint()
    {
        if (MovementTypes.Crouch == currentMovementType) return false;

        // Checks if players wants to sprint and if he pressed a directional key
        if (wantsCrouch)
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
            case CrouchTypes.Hold when wantsCrouch:

                // Sets the MovementType to crouched
                return SetCurrentMovementType(MovementTypes.Crouch);
            
            // Will uncrouch the player if there is no input
            case CrouchTypes.Hold:
            {
                if (MovementTypes.Crouch == currentMovementType)
                {
                    // Sets the MovementType to stand
                    return SetCurrentMovementType(MovementTypes.Stand);
                }
                    
                break;
            }

            case CrouchTypes.Toggle:
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

    private void SetMoveAmount(Vector3 moveRaw3D)
    {
        moveAmount = Vector3.SmoothDamp(moveAmount, moveRaw3D * currentSpeed, ref smoothMoveVelocity, smoothTime);
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
