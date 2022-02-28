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
        if (PV.IsMine) return;
        
        Destroy(GetComponentInChildren<Camera>().gameObject);
        Destroy(RB);
    }
<<<<<<< HEAD
=======
    
>>>>>>> 77a9b68f2166e7ea953904f6c622a94623cca372
    private void Update() // Don't touch !
    {
        if (!PV.IsMine) return;
        
        Look();
        Move();
        UpdateAppearance(); // updates the appearance based on the MovementType
    }
    
    private void FixedUpdate()
    {
        if (!PV.IsMine) return;
        
        RB.MovePosition(RB.position + transform.TransformDirection(PM.moveAmount) * Time.fixedDeltaTime);
        UpdateHitbox();
    }
    
    #endregion

    private void Look()
    {
        PM.Look();
    }

    private void Move() // Moves the player based on ZQSD inputs
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        // Is used to tell the network if an update is required (appearance, hitbox)
        bool changeOccured = false;

        // Updates the sprinting state
        changeOccured |= UpdateSprint();

        // Updates the crouching state
        changeOccured |= UpdateCrouch();

        // Updates the walk state
        changeOccured |= UpdateWalk(moveDir);
        
        // Updates the jump feature
        changeOccured |= UpdateJump();

        // Updates the speed based on the MovementType
        PM.UpdateSpeed(currentMovementType);

        // Sets the new movement vector based on previous methods
        PM.SetMoveAmount(moveDir);
    }

    private bool UpdateCrouch()
    {
        // Checks the current crouch mode (toggle or hold)
        switch (currentCrouchType)
        {
            // Will crouch the player
            case CrouchModes.Hold when Input.GetButton("Crouch"):

                // Sets the MovementType to crouched
                return SetCurrentMovementType(MovementTypes.Crouch);

                break;

            // Will UNcrouch the player if there is no input
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
                    switch (currentMovementType)
                    {
                        // Is crouched - wants to uncrouch
                        case (MovementTypes.Crouch):

                            // Checks if UNcrouching is possible (example: no obstacles above)
                            if (true)
                            {
                                // Sets the MovementType to stand
                                return SetCurrentMovementType(MovementTypes.Stand);
                            }

                            break;

                        // Is NOT crouched - wants to crouch
                        default:

                            // Sets the MovementType to crouched
                            return SetCurrentMovementType(MovementTypes.Crouch);
                    }
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException("A movement type other than .Stand and .Crouch was entered:" + currentCrouchType.ToString());
        }

        throw new ArgumentOutOfRangeException("A crouch setting mode other than .Hold and .Toggle was entered:" + currentCrouchType.ToString());
    }

    private bool UpdateWalk(Vector3 _moveDir)
    {
        if (MovementTypes.Stand == currentMovementType && Vector3.zero != _moveDir)
        {
            return SetCurrentMovementType(MovementTypes.Walk);
        }

        return false;
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

        return false; // we never know
    }

    private bool UpdateJump()
    {
        PM.Jump();

        return true; // for now, no conditions prevents the player from jumping
    }

    private void UpdateAppearance()
    {
        bool changedOccured = false;
        PA.UpdateAppearance(currentMovementType);
        
        // Network sync: hitbox
        PV.RPC("RPC_UpdateAppearance", RpcTarget.Others, currentMovementType);
    }
    
    private void UpdateHitbox()
    {
        PA.UpdateHitbox(currentMovementType);
    }

    #region Attributes setters

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
    
    #endregion
    
    // Network syncronization
    #region RPCs

    [PunRPC]
    // Syncronizes the appearance
    void RPC_UpdateAppearance(MovementTypes movementType)
    {
        this.UpdateAppearance();
    }

    #endregion
}
