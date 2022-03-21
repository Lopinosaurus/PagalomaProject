using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Photon.Pun;
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
    private PhotonView _photonView;
    
    // Sensitivity
    [Space]
    [Header("Mouse settings")]
    [Range(4f, 128f)]
    [SerializeField] private float mouseSensX = 10f;
    [Range(4f, 128f)]
    [SerializeField] private float mouseSensY = 10f;

    // private float YLookRotation;
    private bool shouldLookAround = true;

    // Mouse input values
    private float _mouseDeltaX;
    private float _mouseDeltaY;

    // Current player values
    private float _rotationX;
    private float _rotationY;
    private float _smoothValueX;
    private float _smoothValueY;
    private const float SmoothTimeX = 0.01f;
    private const float SmoothTimeY = 0.01f;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (!_photonView.IsMine)
        {
            // Enable head components' layers
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _playerControls = _playerController.PlayerControls;

        _playerControls.Player.Look.performed += ctx => _mouseDeltaX = ctx.ReadValue<Vector2>().x;
        _playerControls.Player.Look.performed += ctx => _mouseDeltaY = ctx.ReadValue<Vector2>().y;
        _playerControls.Player.Look.canceled += ctx => _mouseDeltaX = ctx.ReadValue<Vector2>().x;
        _playerControls.Player.Look.canceled += ctx => _mouseDeltaY = ctx.ReadValue<Vector2>().y;
    }


    #endregion
    
    public void Look() // Modifies camera and player rotation
    {
        if (!shouldLookAround) return;

        _rotationY += _mouseDeltaX * mouseSensX;
        _rotationX -= _mouseDeltaY * mouseSensY;

        _rotationX = Mathf.Clamp(_rotationX, -80f, 70f);

        Quaternion rotation = Quaternion.Euler(_rotationX, _rotationY, 0);

        _rotationX = Mathf.SmoothDampAngle(_rotationX, rotation.eulerAngles.x, ref _smoothValueX, SmoothTimeX);
        _rotationY = Mathf.SmoothDampAngle(_rotationY, rotation.eulerAngles.y, ref _smoothValueY, SmoothTimeY);

        camHolder.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        transform.rotation = Quaternion.Euler(0, _rotationY, 0);

        // transform.Rotate(Vector3.up * (Input.GetAxisRaw("Mouse X") * mouseSensY));
        //
        // YLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensX;
        // YLookRotation = Mathf.Clamp(YLookRotation, -70f, 80f);
        //
        // transform.localEulerAngles = Vector3.left * YLookRotation;
    }
}
