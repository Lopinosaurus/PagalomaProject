using System;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject cameraHolder = null;
    [SerializeField] public GameObject _bodyHitBox = null;
    [SerializeField] public CapsuleCollider playerCol = null;

    [Space]
    [Header ("Mouse settings")]
    [SerializeField] private float mouseSensHorizontal = 3f;
    [SerializeField] private float mouseSensVertical = 3f;

    [Space]
    [Header ("Movement settings")]
    [SerializeField] private MovementTypes currentMovementType = MovementTypes.Walk;
    [SerializeField] public CrouchInputMode currentCrouchType = CrouchInputMode.Hold;
    [SerializeField]
    private enum MovementTypes
    {
        Walk,
        Sprint,
        Crouch
    };
    [SerializeField] public enum CrouchInputMode
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
    [SerializeField] private float jumpForce;
    [SerializeField] private float smoothTime;

    private float verticalLookRotation;
    private bool grounded;
    private Vector3 smoothMoveVelocity;
    private Vector3 moveAmount;
    
    private Rigidbody rb;
    private PhotonView PV;
    void Awake() // Don't touch !
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
    }

    void Update() // Don't touch !
    {
        if (PV.IsMine)
        {
           UpdateCameraRotation();
           Move();
           Jump(); 
        }
    }

    private void Start() // Don't touch !
    {
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }

        // Added this! Tell me if I was wrong(needed for crouching to modify player's height)
        playerCol = _bodyHitBox.GetComponent<CapsuleCollider>();
        normalHeight = playerCol.height;
        crouchedHeight = normalHeight / 2;
    }

    void UpdateCameraRotation() // Modifies camera and player rotation
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensVertical);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensHorizontal;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    void Move() // Moves the player based on ZQSD inputs
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;



        // Updates the crouching state (takes into account the input and the toggle/hold mode)
        UpdateCrouch();
        // Updates the sprinting state
        UpdateSprint();

        // Updates the speed based on the MovementType
        currentSpeed = UpdateSpeed();



        // Actually moves the player
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * currentSpeed, ref smoothMoveVelocity, smoothTime);
    }

    void UpdateCrouch()
    {
        if (MovementTypes.Sprint == currentMovementType) return;

        // Checks the current crouch mode (toggle or hold)
        switch (currentCrouchType)
        {
            // HOLD INPUT MODE
            case (CrouchInputMode.Hold):

                if (Input.GetButton("Crouch"))
                {
                    // Sets the height to crouched height
                    playerCol.height = crouchedHeight;

                    // Sets the MovementType to crouched
                    SetCurrentMovementType(MovementTypes.Crouch);
                }
                else
                {
                    // Checks if UNcrouching is possible (no obstacles above, is actually in crouch mode)
                    if (MovementTypes.Crouch == currentMovementType)
                    {
                        // Sets the height to normal height
                        playerCol.height = normalHeight;

                        // Sets the MovementType to walk
                        SetCurrentMovementType(MovementTypes.Walk);
                    }
                }

                break;

            // TOGGLE INPUT MODE
            case (CrouchInputMode.Toggle):

                if (Input.GetButtonDown("Crouch"))
                {
                    // Checks the current state of crouch
                    switch (currentMovementType)
                    {
                        // Is crouched - wants to uncrouch
                        case (MovementTypes.Crouch):

                            // Checks if UNcrouching is possible (no obstacles above, is actually in crouch mode)
                            if (MovementTypes.Crouch == currentMovementType)
                            {
                                // Sets the height to normal height
                                playerCol.height = normalHeight;

                                // Sets the MovementType to walk
                                SetCurrentMovementType(MovementTypes.Walk);
                            }

                            break;

                        // Is NOT crouched - wants to crouch
                        default:

                            // Sets the height to crouched height
                            playerCol.height = crouchedHeight;

                            // Sets the MovementType to crouched
                            SetCurrentMovementType(MovementTypes.Crouch);

                            break;
                    }
                }

                break;
        }
    }

    void UpdateSprint()
    {
        if (MovementTypes.Crouch == currentMovementType) return;

        // Checks if players wants to sprint
        if (Input.GetButton("Sprint"))
        {
            SetCurrentMovementType(MovementTypes.Sprint);
        }
        else
        {
            SetCurrentMovementType(MovementTypes.Walk);
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }

    float UpdateSpeed()
    {
        switch (currentMovementType)
        {
            case MovementTypes.Walk:
                return walkSpeed;
            case MovementTypes.Sprint:
                return sprintSpeed;
            case MovementTypes.Crouch:
                return crouchSpeed;
        }

        return currentSpeed;
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
