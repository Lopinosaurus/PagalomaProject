using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    #endregion
    
    public void Look() // Modifies camera and player rotation
    {
        transform.Rotate(Vector3.up * (Input.GetAxisRaw("Mouse X") * mouseSensVertical));

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensHorizontal;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }
}
