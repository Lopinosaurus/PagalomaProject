using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    [SerializeField]
    private Camera cam;
    
    private Vector3 velocity;
    private Vector3 rotation;
    private float cameraRotationX = 0f;
    private float currentCameraRotationX = 0f;
    private Vector3 thrusterVelocity;
    public float cameraRotationLimit = 85f;
    
    private Rigidbody rb;
    PhotonView view;

    void Awake()
    {
        view = GetComponent<PhotonView>();
    }
    

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!view.IsMine)
        {
            // Destroy(GetComponentInChildren<Camera>().gameObject); // Remove gun too
            Destroy(cam); // Doesn't destroy audio listener
            Destroy(rb);
        }
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }
    
    public void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }
    
    public void RotateCamera(float _cameraRotationX)
    {
        cameraRotationX = _cameraRotationX;
    }

    public void ApplyThruster(Vector3 _thrusterVelocity)
    {
        thrusterVelocity = _thrusterVelocity;
    }

    private void FixedUpdate()
    {
        if (!view.IsMine) return;
        PerformMovement();
        PerformRotation();
    }

    private void PerformMovement()
    {
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }

        if (thrusterVelocity != Vector3.zero)
        {
            rb.AddForce(thrusterVelocity * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    private void PerformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        // On calcule la rotation de la caméra
        currentCameraRotationX -= cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);
        // On applique la rotation de la caméra
        cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }
}
