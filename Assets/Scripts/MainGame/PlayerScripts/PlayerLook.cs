using System;
using System.Collections;
using Photon.Pun;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace MainGame.PlayerScripts
{
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
        [Space] [Header("Mouse settings")] [Range(4f, 128f)] [SerializeField]
        private float mouseSensX = 10f;

        [Range(4f, 128f)] [SerializeField] private float mouseSensY = 10f;

        // Mouse input values
        private float _mouseDeltaX;
        private float _mouseDeltaY;

        // Current player values
        private float _rotationX;
        private float _rotationY;
        private const float SmoothTimeX = 0.01f;
    
        [Space][Header("Shake settings")]
        [SerializeField] [Range(0.0001f, 0.01f)] private float probShakeMultiplier = 0.01f;
        [SerializeField] [Range(0.1f, 10f)] private float shakeMultiplier = 4;

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

        /*private void Update()
        {
            // Try bake occlusion
            if (Input.GetKeyDown(KeyCode.M))
            {
                try
                {
                    Debug.Log("Started occlusion baking");
                    StaticOcclusionCulling.Compute();  
                }
                catch
                {
                    Debug.LogWarning("Couldn't start occlusion baking");
                }
            }
        }
        */

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

            var localRotationEuler = camHolder.transform.localRotation.eulerAngles;
            localRotationEuler.x = _rotationX;
            camHolder.transform.localRotation = Quaternion.Euler(localRotationEuler);

            var rotationEuler = _characterController.transform.rotation.eulerAngles;
            rotationEuler.y = _rotationY;
            _characterController.transform.rotation = Quaternion.Euler(rotationEuler);
        }

        public Coroutine StartShake(float duration)
        {
            return StartCoroutine(Shake(duration));
        }
        
        private IEnumerator Shake(float duration)
        {
            float timer = 0;
            float prob = 0;
            Quaternion newRot = Random.rotation;
        
            // Final values
            var finalRotEulerAngles = camHolder.localRotation.eulerAngles;
            finalRotEulerAngles.z = 0;
            finalRotEulerAngles.y = 0;
            var finalRot = Quaternion.Euler(finalRotEulerAngles);
        
            // Twist angle
            while (timer < duration)
            {
                timer += Time.deltaTime;

                float strength = (duration - timer) / duration * shakeMultiplier;
            
                Quaternion camHolderLocalRotation = camHolder.localRotation;

                if (Random.Range(0, 1) > prob)
                {
                    prob += Time.deltaTime * probShakeMultiplier;
                }
                else
                {
                    prob = 0;
                    newRot = Random.rotation;
                }

                // Move towards the angle
                camHolder.localRotation = Quaternion.RotateTowards(
                    camHolderLocalRotation,
                    newRot,
                    strength);
            
                yield return null;
            }

            // Restore normal angle
            timer = 10;
            while (Quaternion.Angle(camHolder.localRotation, finalRot) > 0.5f && timer > 0)
            {
                camHolder.localRotation = Quaternion.Slerp(camHolder.localRotation, finalRot, Time.deltaTime);

                timer -= Time.deltaTime;
                yield return null;
            }
        
            ResetYZCam();
        }

        public void ResetYZCam()
        {
            Vector3 rot = camHolder.localRotation.eulerAngles;
            rot.y = 0;
            rot.z = 0;
            camHolder.localRotation = Quaternion.Euler(rot);
        }
    }
}