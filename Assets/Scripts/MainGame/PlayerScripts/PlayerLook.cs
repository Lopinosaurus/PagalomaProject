using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]

public class PlayerLook : MonoBehaviour
{
    #region Attributes

    [SerializeField] private GameObject cameraHolder = null;

    // Sensitivity
    [Space]
    [Header("Mouse settings")]
    [Range(0.01f, 50f)]
    [SerializeField] private float mouseSensHorizontal = 3f;
    [Range(0.01f, 50f)]
    [SerializeField] private float mouseSensVertical = 3f;
    private float verticalLookRotation;

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
        }

        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    #endregion
    
    public void Look() // Modifies camera and player rotation
    {
        transform.Rotate(Vector3.up * (Input.GetAxisRaw("Mouse X") * mouseSensVertical));

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensHorizontal;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -70f, 80f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }
    
}
