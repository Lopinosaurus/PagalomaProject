using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using Random = UnityEngine.Random;

namespace MainGame.PlayerScripts
{
    public class PlayerLook : MonoBehaviour
    {
        #region Attributes

        // Components
        [SerializeField] private Transform camHolder;
        private Transform _cam;
        private PlayerInput _playerInput;
        private RotationConstraint _rotationConstraint;
        private PhotonView _photonView;
        private FootstepEffect _footstepEffect;
        private PlayerAnimation _playerAnimation;
        private PlayerMovement _playerMovement;
        private CharacterController _characterController;

        // Sensitivity
        [Space] [Header("Mouse settings")]
        private float mouseSensX = 10f;
        private float mouseSensY = 10f;

        // Mouse input values
        private float _mouseDeltaX;
        private float _mouseDeltaY;
        public float turnDeltaY => _mouseDeltaX * mouseSensX * canTurnSidesMult;
        private float canTurnSidesMult => (!canTurnSides ? 0.1f : 1);

        // Current player values
        private float _rotationX;
        private float _rotationY;
        private const float SmoothTimeX = 0.01f;
    
        // Shake settings
        [Space][Header("Shake settings")]

        // Jump settings
        private bool canTurnSides = true;

        // Head settings
        [SerializeField] private Transform BodyToRotate;
        private float deltaRotation;
        private float rotationRef;
        private const float bodyResetRotStrength = 12;
        [SerializeField] [Range(0, 90)] private float rotationThreshold = 60f;
        private bool isMoving;
        
        // Head Bob settings
        private float amplitude = 0.1f;

        public void HeadBob()
        {
            float quot = Mathf.PI / _footstepEffect.MaxDistance;
            float piHalf = Mathf.PI * 0.5f;

            Vector3 pos = camHolder.position;

            pos.y -= Mathf.Cos(_footstepEffect.PlayerDistanceCounter * quot + piHalf) * amplitude;
            _cam.position = pos;
            var localPosition = _cam.localPosition;
            localPosition.z = 0;
            _cam.localPosition = localPosition;
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerInput = GetComponent<PlayerInput>();
            _playerAnimation = GetComponent<PlayerAnimation>();
            _playerMovement = GetComponent<PlayerMovement>();
            _photonView = GetComponent<PhotonView>();
            _footstepEffect = GetComponent<FootstepEffect>();
            _cam = GetComponentInChildren<Camera>().transform;
            _rotationConstraint = GetComponentInChildren<RotationConstraint>();
            
            if (_photonView.IsMine)
            {
                _rotationConstraint.locked = false;
                _rotationConstraint.rotationAtRest = new Vector3(200, 0, 0);

                _rotationConstraint.SetSource(0, new ConstraintSource());
            }
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _playerInput.actions["Look"].performed += ctx => _mouseDeltaX = ctx.ReadValue<Vector2>().x;
            _playerInput.actions["Look"].performed += ctx => _mouseDeltaY = ctx.ReadValue<Vector2>().y;
            _playerInput.actions["Look"].canceled += ctx => _mouseDeltaX = ctx.ReadValue<Vector2>().x;
            _playerInput.actions["Look"].canceled += ctx => _mouseDeltaY = ctx.ReadValue<Vector2>().y;
        }

        #endregion

        public void Look() // Modifies camera and player rotation
        {
            _rotationY += _mouseDeltaX * mouseSensX * canTurnSidesMult;
            _rotationX -= _mouseDeltaY * mouseSensY * canTurnSidesMult;

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

            var localRotationEuler = camHolder.localRotation.eulerAngles;
            localRotationEuler.x = _rotationX;
            camHolder.localRotation = Quaternion.Euler(localRotationEuler);

            var rotationEuler = _characterController.transform.rotation.eulerAngles;
            rotationEuler.y = _rotationY;
            _characterController.transform.rotation = Quaternion.Euler(rotationEuler);
        }

        public void StartShake(float duration, float shakeStrength)
        {
            shakeStrength = shakeStrength < 0 ? 0 : shakeStrength * 5;
            StartCoroutine(Shake(duration, shakeStrength));
        }
        
        private IEnumerator Shake(float duration, float shakeStrength)
        {
            float timer = 0;

            // Twist angle
            while (timer < duration)
            {
                timer += Time.deltaTime;

                float strength = (duration - timer) / duration;
                float deltaAngle = shakeStrength * strength;

                deltaAngle *= 10f * strength;
                deltaAngle *= 0.5f + Mathf.Cos(strength * 2 * Mathf.PI) * 0.5f;
                
                Vector3 forwardCamHolder = camHolder.InverseTransformDirection(camHolder.forward);

                camHolder.Rotate(forwardCamHolder, deltaAngle);
                var localRotationEulerAngles = camHolder.localRotation.eulerAngles;
                localRotationEulerAngles.y = 0;
                camHolder.localRotation = Quaternion.Euler(localRotationEulerAngles);

                yield return null;
            }

            var finalRotEulerAngles = new Vector3(camHolder.localRotation.eulerAngles.x, 0, 0);
            var finalRot = Quaternion.Euler(finalRotEulerAngles);
            // Restore normal angle
            timer = 10;
            while (Mathf.Abs(Quaternion.Angle(camHolder.localRotation, finalRot)) != 0 && timer > 0)
            {
                // Final values
                var localRotation = camHolder.localRotation;
                finalRotEulerAngles = new Vector3(localRotation.eulerAngles.x, 0, 0);
                finalRot = Quaternion.Euler(finalRotEulerAngles);
                
                localRotation = Quaternion.Lerp(localRotation, finalRot, 0.1f);
                camHolder.localRotation = localRotation;

                timer += Time.deltaTime;
                
                yield return null;
            }
        
            ResetZCam();
        }

        private void ResetZCam()
        {
            Vector3 rot = camHolder.localRotation.eulerAngles;
            rot.z = 0;
            camHolder.localRotation = Quaternion.Euler(rot);
        }

        public void HeadRotate()
        {
            // X-axis applies at all times (component)

            // Y-axis applies when the player stops moving
            isMoving = Mathf.Abs(_playerAnimation.velocityX) > 0.1f ||
                       Mathf.Abs(_playerAnimation.velocityZ) > 0.1f;
            
            if (isMoving)
            {
                rotationRef = transform.rotation.eulerAngles.y;
                RotateBodyY(0);
            }
            else
            {
                // This delta is the difference between the last rotation while moving and the current rotation
                float rotation = transform.rotation.eulerAngles.y;
                deltaRotation = Mathf.DeltaAngle(rotation, rotationRef);
                
                // If the body rotates beyond that limit, we re-adapt
                if (Mathf.Abs(deltaRotation) > rotationThreshold)
                {
                    var min = rotation - rotationThreshold;
                    var max = rotation + rotationThreshold;

                    if (rotationRef > max && deltaRotation < 0)
                    {
                        rotationRef -= 360;
                    }
                    if (rotationRef < min && deltaRotation > 0)
                    {
                        rotationRef += 360;
                    }

                    
                    rotationRef = Mathf.Clamp(rotationRef, min, max);
                }
                else
                {
                    RotateBodyY(deltaRotation, true);
                }
            }
        }

        private void RotateBodyY(float rotY, bool isInstant = false)
        {
            Vector3 localRotHips = BodyToRotate.localRotation.eulerAngles;
            float resetRotStrength = isInstant ? 1 : bodyResetRotStrength * Time.deltaTime;
            localRotHips.y = Mathf.LerpAngle(localRotHips.y, rotY, resetRotStrength);
            
            BodyToRotate.transform.localRotation = Quaternion.Euler(localRotHips);
        }

        public void LockViewJump(bool locked) => canTurnSides = !locked;

        public void LocalPostProcessing(PostProcessVolume postProcessVolume, float duration)
        {
            // Attach gameObject to player
            GameObject localVolume = new GameObject($"localVolume ({duration})");
            localVolume.transform.SetParent(transform);
            
            // Attach spawner
            PostProcessVolumeSpawner spawner = localVolume.AddComponent<PostProcessVolumeSpawner>();
            spawner.timer = duration;
            spawner.enabled = true;

            // Attach postprocess
            postProcessVolume.weight = 1;
            postProcessVolume.priority = 1;
            postProcessVolume.isGlobal = true;
            postProcessVolume.transform.SetParent(localVolume.transform);
        }
    }
}