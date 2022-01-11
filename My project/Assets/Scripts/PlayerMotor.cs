using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    [SerializeField]
    private Camera cam;


    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private Vector3 cameraRotation = Vector3.zero;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

    }

    // Gets a movement vector
    public void Move (Vector3 _velocity)
    {
        velocity = _velocity;

    }

    // Gets a rotational vector
    public void Rotate (Vector3 _rotation)
    {
        rotation = _rotation;
    }

    // Gets a rotational vector for the camera
    public void RotateCamera (Vector3 _cameraRotation)
    {
        cameraRotation = _cameraRotation;
    }

    // Runs every physics iteration
    private void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
    }

    // Perfoms movement based on velocity variable
    void PerformMovement ()
    {
        if (velocity != Vector3.zero)
        {
            // This function also checks if there is some collision in the way
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
    }

    // Performs rotation and camera rotation
    void PerformRotation ()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler (rotation));
        if (cam != null)
        {
            // the "-" means that dragging the mouse down will lift the camera up and vice-versa
            cam.transform.Rotate(-cameraRotation);
        }
    }

}
