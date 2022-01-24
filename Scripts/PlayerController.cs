using System;
using System.Security.Cryptography;
using UnityEngine;
using Photon.Pun;

// Lignes optionnelles
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(ConfigurableJoint))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed = 3f;

    [SerializeField] private float mouseSensitivityX = 3f;
    [SerializeField] private float mouseSensitivityY = 3f;
    [SerializeField] private float thrusterForce = 1000f;

    [Header("Joint Options")]
    [SerializeField] private float jointSpring = 20f;
    [SerializeField] private float jointMaxForce = 60f;
    
    private PlayerMotor motor;
    private ConfigurableJoint joint;

    PhotonView view;

    void Awake()
    {
        view = GetComponent<PhotonView>();
    }
    
    private void Start()
    {
        motor = GetComponent<PlayerMotor>();
        joint = GetComponent<ConfigurableJoint>();
        SetJointSettings(jointSpring);
        
    }

    private void Update()
    {
        if (!view.IsMine) return;
        // Calculer la vélocité (vitesse) du mouvement de notre joueur.
        float xMov = Input.GetAxisRaw("Horizontal");
        float zMov = Input.GetAxisRaw("Vertical");

        Vector3 moveHorizontal = transform.right * xMov;
        Vector3 moveVertical = transform.forward * zMov;

        Vector3 velocity = (moveHorizontal + moveVertical).normalized * speed;

        motor.Move(velocity);
        
        // On calcule la rotation du joueur en un Vector3
        float yRot = Input.GetAxisRaw("Mouse X");
        Vector3 rotation = new Vector3(0, yRot, 0) * mouseSensitivityX;
        motor.Rotate(rotation);
        
        // On calcule la rotation de la camera en un Vector3
        float xRot = Input.GetAxisRaw("Mouse Y");
        float cameraRotationX = xRot * mouseSensitivityY;
        motor.RotateCamera(cameraRotationX);
    
        // Calcule de la force du jetpack
        Vector3 thrusterVelocity = Vector3.zero;
        // Applique la variable thrusterForce / utilisation du jetpack
        if (Input.GetButton("Jump"))
        {
            thrusterVelocity = Vector3.up * thrusterForce;
            SetJointSettings(0f);
        } else
        {
            SetJointSettings(jointSpring);
        }

        motor.ApplyThruster(thrusterVelocity);
    }

    private void SetJointSettings(float _jointSpring)
    {
        joint.yDrive = new JointDrive { positionSpring = _jointSpring, maximumForce = jointMaxForce };
    }
}
