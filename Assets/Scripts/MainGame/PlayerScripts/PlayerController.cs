using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;
// ReSharper disable All

[RequireComponent(typeof(PlayerAnimation))]

public class PlayerController : MonoBehaviour
{
    #region Attributes
    private Rigidbody RB;
    private PhotonView PV;
    private PlayerMovement PM;
    private PlayerAnimation PA;
    
    [Space]
    [Header("Movement settings")]
    [SerializeField] public MovementTypes currentMovementType = MovementTypes.Stand;
    [SerializeField] public CrouchModes currentCrouchType = CrouchModes.Hold;
    [SerializeField] public bool grounded;
    
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
        RB = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        
        // Player's AnimationController instance
        PA = GetComponent<PlayerAnimation>();
        PM = GetComponent<PlayerMovement>();
    }
    
    private void Start() // Don't touch !
    {
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(RB);

            return;
        }
    }
    private void Update() // Don't touch !
    {
        if (PV.IsMine)
        {
            Look();
            Move();
            UpdateAppearance(); // updates the appearance based on the MovementType
        }
    }
    
    private void FixedUpdate()
    {
        if (PV.IsMine)
        {
            RB.MovePosition(RB.position + transform.TransformDirection(PM.moveAmount) * Time.fixedDeltaTime);
            UpdateHitbox();
        }
    }
    
    #endregion

    private void Look()
    {
        PM.Look();
    }

    private void Move() // Moves the player based on ZQSD inputs
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        // Updates the sprinting state
        UpdateSprint();

        // Updates the crouching state
        UpdateCrouch();

        // Updates the walk state
        UpdateWalk(moveDir);
        
        // Updates the jump feature
        UpdateJump();

        // Updates the speed based on the MovementType
        PM.UpdateSpeed(currentMovementType);

        // Sets the new movement vector based on previous methods
        PM.SetMoveAmount(moveDir);
    }

    private void UpdateCrouch()
    {
        // Checks the current crouch mode (toggle or hold)
        switch (currentCrouchType)
        {
            // Will crouch the player
            case CrouchModes.Hold when Input.GetButton("Crouch"):

                // Sets the MovementType to crouched
                SetCurrentMovementType(MovementTypes.Crouch);

                break;

            // Will UNcrouch the player if there is no input
            case CrouchModes.Hold:
            {
                    if (MovementTypes.Crouch == currentMovementType)
                    {
                        // Sets the MovementType to stand
                        SetCurrentMovementType(MovementTypes.Stand);
                    }
                    
                    break;
            }

            case CrouchModes.Toggle:
            {
                if (Input.GetButtonDown("Crouch"))
                {
                    // Checks the current state of crouch
                    switch (currentMovementType)
                    {
                        // Is crouched - wants to uncrouch
                        case (MovementTypes.Crouch):

                            // Checks if UNcrouching is possible (example: no obstacles above)
                            if (true)
                            {
                                // Sets the MovementType to stand
                                SetCurrentMovementType(MovementTypes.Stand);
                            }

                            break;

                        // Is NOT crouched - wants to crouch
                        default:

                            // Sets the MovementType to crouched
                            SetCurrentMovementType(MovementTypes.Crouch);

                                Debug.Log("Enabled crouched with toggle mode on !");
                            break;
                    }
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateWalk(Vector3 _moveDir)
    {
        if (MovementTypes.Stand == currentMovementType && Vector3.zero != _moveDir)
        {
            SetCurrentMovementType(MovementTypes.Walk);
        }
    }
    
    private void UpdateSprint()
    {
        if (MovementTypes.Crouch == currentMovementType) return;

        // Checks if players wants to sprint and if he pressed a directional key
        if (Input.GetButton("Sprint") && (0 != Input.GetAxisRaw("Horizontal") || 0 != Input.GetAxisRaw("Vertical")))
        {
            SetCurrentMovementType(MovementTypes.Sprint);
        }
        else
        {
            SetCurrentMovementType(MovementTypes.Stand);
        }
    }

    private void UpdateJump()
    {
        PM.Jump();
    }

    private void UpdateAppearance()
    {
        PA.UpdateAppearance(currentMovementType);
    }
    private void UpdateHitbox()
    {
        PA.UpdateHitbox(currentMovementType);
    }

    #region Attributes setters
    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    private void SetCurrentMovementType (MovementTypes _currentMovementType)
    {
        currentMovementType = _currentMovementType;
    }
    
    #endregion
}
