using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PlayerController))]

public class PlayerMovement : MonoBehaviour
{
    #region Attributes
    
    // External GameObects and components
    [SerializeField] private GameObject cameraHolder = null;
    private Rigidbody RB = null;
    private PhotonView PV = null;
    private PlayerController PC = null;
    private CapsuleCollider CC = null;

    
    // Movement speeds
    [Space]
    [Header("Player speed settings")]
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float currentSpeed;
    
    // Sensitivity
    [Space]
    [Header("Mouse settings")]
    [SerializeField] private float mouseSensHorizontal = 3f;
    [SerializeField] private float mouseSensVertical = 3f;
    
    // Heights
    [Space]
    [Header("Player height settings")]
    private float crouchedHeight;
    private float standingHeight;
    
    // Jump values
    [Space]
    [Header("Player jump settings")]
    [SerializeField] private float jumpForce = 300f;
    [SerializeField] private float smoothTime = 0.15f;
    
    private float verticalLookRotation;
    private Vector3 smoothMoveVelocity;
    public Vector3 moveAmount;
    
    #endregion

    #region Unity methods

    void Awake() // Don't touch !
    {
        RB = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        PC = GetComponent<PlayerController>();
    }

    private void Start()
    {
        var height = CC.height;
        crouchedHeight = height / 2;
        standingHeight = height;
    }

    private void FixedUpdate()
    {
        if (PV.IsMine)
        {
            RB.MovePosition(RB.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
        }
    }

    #endregion
    
    public void Look() // Modifies camera and player rotation
    {
        transform.Rotate(Vector3.up * (Input.GetAxisRaw("Mouse X") * mouseSensVertical));

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensHorizontal;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    public void Jump()
    {
        if (Input.GetButtonDown("Jump") && PC.grounded)
        {
            RB.AddForce(transform.up * jumpForce);
        }
    }
    
    public void UpdateSpeed(PlayerController.MovementTypes currentMovementType)
    {
        currentSpeed = currentMovementType switch
        {
            PlayerController.MovementTypes.Stand => 0f,
            PlayerController.MovementTypes.Crouch => crouchSpeed,
            PlayerController.MovementTypes.Walk => walkSpeed,
            PlayerController.MovementTypes.Sprint => sprintSpeed,
            _ => currentSpeed
        };
    }
    
    public void UpdateHitbox(PlayerController.MovementTypes currentMovementType)
    {
        // Updates player's hitbox about crouch
        #region Crouch management
        
        float desiredHeight = standingHeight;
        if (PlayerController.MovementTypes.Crouch == currentMovementType) desiredHeight = crouchedHeight;

        Vector3 camPosition = cameraHolder.transform.localPosition;
        if (CC.height != desiredHeight)
        {
            AdjustHeight(desiredHeight);
            camPosition.y = CC.height;
        }
        
        cameraHolder.transform.localPosition = camPosition;

        #endregion
        
        
        
    }

    public void SetMoveAmount(Vector3 moveDir)
    {
        // Finally moves the player
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * currentSpeed, ref smoothMoveVelocity, smoothTime);
    }
    
    // Sets the hitbox's height to the input value progressively
    private void AdjustHeight(float height)
    {
        // Calculates the current center of the player
        float center = height / 2f;

        CC.height = Mathf.LerpUnclamped(CC.height, height, crouchSpeed);
        CC.center = Vector3.LerpUnclamped(CC.center, new Vector3(0, center, 0), crouchSpeed);
    }
}
