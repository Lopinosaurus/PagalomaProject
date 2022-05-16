using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerLook : MonoBehaviour
{
    #region Attributes

    [SerializeField] private Transform camHolder;
    private PlayerControls _playerControls;
    private PlayerController _playerController;
    private PlayerInput _playerInput;
    private PhotonView _photonView;
    private CharacterController _characterController;
    
    // Sensitivity
    [Space]
    [Header("Mouse settings")]
    [Range(4f, 128f)]
    [SerializeField] private float mouseSensX = 10f;
    [Range(4f, 128f)]
    [SerializeField] private float mouseSensY = 10f;

    // Mouse input values
    private float _mouseDeltaX;
    private float _mouseDeltaY;

    // Current player values
    private float _rotationX;
    private float _rotationY;
    private const float SmoothTimeX = 0.01f;
    
    #endregion

    #region Unity Methods

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _characterController = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
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

        _playerInput.actions["Look"].performed += ctx => _mouseDeltaX = ctx.ReadValue<Vector2>().x;
        _playerInput.actions["Look"].performed += ctx => _mouseDeltaY = ctx.ReadValue<Vector2>().y;
        _playerInput.actions["Look"].canceled += ctx => _mouseDeltaX = ctx.ReadValue<Vector2>().x;
        _playerInput.actions["Look"].canceled += ctx => _mouseDeltaY = ctx.ReadValue<Vector2>().y;
    }


    #endregion
    
    public void Look() // Modifies camera and player rotation
    {
        _rotationY += _mouseDeltaX * mouseSensX;
        _rotationX -= _mouseDeltaY * mouseSensY;
        
        _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);

        float _ = 0f;
        if (_rotationX < -70f)
        {
            _rotationX = Mathf.SmoothDampAngle(_rotationX, -70f, ref _, SmoothTimeX);
        }
        else if (_rotationX > 80f)
        {
             _rotationX = Mathf.SmoothDampAngle(_rotationX, 80f, ref _, SmoothTimeX);
        }

        /*_rotationX = Mathf.SmoothDampAngle(_rotationX, rotation.eulerAngles.x, ref _smoothValueX, SmoothTimeX);
        _rotationY = Mathf.SmoothDampAngle(_rotationY, rotation.eulerAngles.y, ref _smoothValueY, SmoothTimeY);*/

        camHolder.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        _characterController.transform.rotation = Quaternion.Euler(0, _rotationY, 0);
    }
}
