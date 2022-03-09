using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]

public class PlayerLook : MonoBehaviour
{
    #region Attributes

    [SerializeField] private Transform camHolderTransform;
    
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

    #endregion

    #region Unity Methods

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #endregion
    
    public void Look() // Modifies camera and player rotation
    {
        if (!shouldLookAround) return;

        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        rotationY += mouseX * mouseSensX;
        rotationX -= mouseY * mouseSensY;

        rotationX = Mathf.Clamp(rotationX, -80f, 70f);

        camHolderTransform.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        transform.Rotate(Vector3.up * camHolderTransform.localRotation.eulerAngles.y);
        // transform.Rotate(Vector3.up * (Input.GetAxisRaw("Mouse X") * mouseSensY));
        //
        // YLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensX;
        // YLookRotation = Mathf.Clamp(YLookRotation, -70f, 80f);
        //
        // transform.localEulerAngles = Vector3.left * YLookRotation;
    }
    
}
