using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerController))]

public class PlayerLook : MonoBehaviour
{
    #region Attributes

    [SerializeField] private Transform camHolder;
    [SerializeField] private PlayerControls _playerControls;
    private PlayerController _playerController;
    
    // Sensitivity
    [Space]
    [Header("Mouse settings")]
    [Range(0.01f, 8f)]
    [SerializeField] private float mouseSensX = 3f;
    [Range(0.01f, 8f)]
    [SerializeField] private float mouseSensY = 3f;

    // private float YLookRotation;
    private bool shouldLookAround = true;

    // Mouse input values
    private float mouseDeltaX;
    private float mouseDeltaY;
    private Vector2 mouseDelta;

    // Current player values
    private float rotationX;
    private float rotationY;
    private float smoothValueX;
    private float smoothValueY;
    private float smoothTimeX = 0.01f;
    private float smoothTimeY = 0.01f;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerControls = _playerController.playerControls;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _playerControls.Player.Look.performed += ctx => mouseDeltaX = ctx.ReadValue<Vector2>().x;
        _playerControls.Player.Look.performed += ctx => mouseDeltaY = ctx.ReadValue<Vector2>().y;
        _playerControls.Player.Look.canceled += ctx => mouseDeltaX = ctx.ReadValue<Vector2>().x;
        _playerControls.Player.Look.canceled += ctx => mouseDeltaY = ctx.ReadValue<Vector2>().y;
    }


    #endregion
    
    public void Look() // Modifies camera and player rotation
    {
        if (!shouldLookAround) return;

        // mouseX = Input.GetAxisRaw("Mouse X");
        // mouseY = Input.GetAxisRaw("Mouse Y");
        
        Debug.Log("mouse Delta is:" + mouseDelta);
        Debug.Log("mouse is " + (_playerControls.Player.Look.inProgress ? "": "NOT") + "moving");
        
        rotationY += mouseDeltaX * mouseSensX;
        rotationX -= mouseDeltaY * mouseSensY;

        rotationX = Mathf.Clamp(rotationX, -80f, 70f);

        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);

        rotationX = Mathf.SmoothDampAngle(rotationX, rotation.eulerAngles.x, ref smoothValueX, smoothTimeX);
        rotationY = Mathf.SmoothDampAngle(rotationY, rotation.eulerAngles.y, ref smoothValueY, smoothTimeY);

        camHolder.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation = Quaternion.Euler(0, rotationY, 0);

        // transform.Rotate(Vector3.up * (Input.GetAxisRaw("Mouse X") * mouseSensY));
        //
        // YLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensX;
        // YLookRotation = Mathf.Clamp(YLookRotation, -70f, 80f);
        //
        // transform.localEulerAngles = Vector3.left * YLookRotation;
    }
}
