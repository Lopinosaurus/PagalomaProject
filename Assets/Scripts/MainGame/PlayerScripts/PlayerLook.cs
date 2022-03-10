using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]

public class PlayerLook : MonoBehaviour
{
    #region Attributes

<<<<<<< Updated upstream
    [SerializeField] private GameObject cameraHolder;

    // Sensitivity
    [Space]
    [Header("Mouse settings")]
    [Range(0.01f, 50f)]
    [SerializeField] private float mouseSensHorizontal = 3f;
    [Range(0.01f, 50f)]
    [SerializeField] private float mouseSensVertical = 3f;
    private float verticalLookRotation;
    private bool shouldLookAround = true;

=======
    [FormerlySerializedAs("camHolderTransform")] [SerializeField] private Transform camHolder;
    [SerializeField] private Transform camOrientation;
    private PlayerControls _playerControls;
    
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
    private float mouseX;
    private float mouseY;

    // Current player values
    private float rotationX;
    private float rotationY;
    private float smoothValueX;
    private float smoothValueY;
    private float smoothTimeX = 0.8f;
    private float smoothTimeY = 0.8f;

>>>>>>> Stashed changes
    #endregion

    #region Unity Methods

    // private void Start() => cameraHolder = GetComponent<GameObject>();
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            shouldLookAround = true;
        }

        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            shouldLookAround = false;
        }
    }

    #endregion
    
    public void Look() // Modifies camera and player rotation
    {
        if (!shouldLookAround) return;
        
        transform.Rotate(Vector3.up * (Input.GetAxisRaw("Mouse X") * mouseSensVertical));

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensHorizontal;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -70f, 80f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }
    
}
