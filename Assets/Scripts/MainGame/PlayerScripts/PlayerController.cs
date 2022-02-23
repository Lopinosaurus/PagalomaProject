using System;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PlayerAnimation))]

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject cameraHolder = null;
    private Rigidbody rb;
    private PhotonView PV;
    private PlayerAnimation PA;

    [Space]
    [Header("Mouse settings")]
    [SerializeField] private float mouseSensHorizontal = 3f;
    [SerializeField] private float mouseSensVertical = 3f;

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

    [Space]
    [Header("Player speed settings")]
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float currentSpeed;

    [Space]
    [Header("Player height settings")]
    [SerializeField] private float crouchedHeight;
    [SerializeField] private float normalHeight;

    [Space]
    [Header("Player jump settings")]
    [SerializeField] private float jumpForce = 300f;
    [SerializeField] private float smoothTime = 0.15f;

    private float verticalLookRotation;
    [SerializeField] private bool grounded;
    private Vector3 smoothMoveVelocity;
    private Vector3 moveAmount;
    
    void Awake() // Don't touch !
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
    }

    private void Start() // Don't touch !
    {
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            // TODO
            // MATHIEU ! JE CROIS QUE CA MARCHE PLUS ICI VU QUE J'AI RESTRUCTURÉ
            // Destroy(cameraHolder.GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);

            return;
        }

        // Player's AnimationController instance
        PA = GetComponent<PlayerAnimation>();
    }
    void Update() // Don't touch !
    {
        if (PV.IsMine)
        {
            Look();
            Move();
            Jump();
        }
    }

    private void Look() // Modifies camera and player rotation
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensVertical);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensHorizontal;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    private void Move() // Moves the player based on ZQSD inputs
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        // Updates the sprinting state
        UpdateSprint();

        // Updates the crouching state
        UpdateCrouch();

        UpdateWalk(moveDir);


        // Updates the speed based on the MovementType
        currentSpeed = UpdateSpeed();

        // Updates the appearance based on the MovementType
        UpdateAppearance();
        UpdateHitbox();

        // Actually moves the player
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * currentSpeed, ref smoothMoveVelocity, smoothTime);
    }

    private void UpdateSprint()
    {
        if (MovementTypes.Crouch == currentMovementType) return;

        // Checks if players wants to sprint
        if (Input.GetButton("Sprint") && (0 != Input.GetAxisRaw("Horizontal") || 0 != Input.GetAxisRaw("Vertical")))
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

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }

    private float UpdateSpeed()
    {
        return currentMovementType switch
        {
            MovementTypes.Stand => 0f,
            MovementTypes.Crouch => crouchSpeed,
            MovementTypes.Walk => walkSpeed,
            MovementTypes.Sprint => sprintSpeed,
            _ => currentSpeed
        };
    }
    private void UpdateAppearance()
    {
        PA.UpdateAppearance(currentMovementType);
    }
    private void UpdateHitbox()
    {
        PA.UpdateHitbox(currentMovementType);
    }

    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    private void SetCurrentMovementType (MovementTypes _currentMovementType)
    {
        currentMovementType = _currentMovementType;
    }

    private void FixedUpdate()
    {
        if (PV.IsMine)
        {
            rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
        }
    }
}
