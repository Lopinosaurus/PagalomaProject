using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
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
        private PhotonView _photonView;
        private FootstepEffect _footstepEffect;
        private PlayerAnimation _playerAnimation;
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
        [SerializeField] [Range(0.0001f, 0.01f)] private float probShakeMultiplier = 0.01f;
        [SerializeField] [Range(0.1f, 10f)] private float shakeMultiplier = 4;
        
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
        private int side = 1;
        private float amplitude = 1;
        
        #endregion

        #region Unity Methods

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerInput = GetComponent<PlayerInput>();
            _playerAnimation = GetComponent<PlayerAnimation>();
            _photonView = GetComponent<PhotonView>();
            _footstepEffect = GetComponent<FootstepEffect>();
            _cam = GetComponentInChildren<Camera>().transform;
        }

        private void Start()
        {
            if (_photonView.IsMine) StartCoroutine(HeadBob());
            
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

        public void StartShake(float duration, float resetSpeed = 1)
        {
            StartCoroutine(Shake(duration, resetSpeed));
        }
        
        private IEnumerator Shake(float duration, float resetSpeed)
        {
            float timer = 0;
            float prob = 0;
            Quaternion newRot = Random.rotation;
        
            // Final values
            var finalRotEulerAngles = camHolder.localRotation.eulerAngles;
            finalRotEulerAngles.z = 0;
            finalRotEulerAngles.y = 0;
            var finalRot = Quaternion.Euler(finalRotEulerAngles);
            resetSpeed = resetSpeed < 1 ? 1 : resetSpeed; 
            
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
                    strength * Time.deltaTime * 100);
            
                yield return null;
            }

            // Restore normal angle
            timer = 10;
            while (Quaternion.Angle(camHolder.localRotation, finalRot) > 0.5f && timer > 0)
            {
                float strength = Mathf.Clamp01(Time.deltaTime * resetSpeed);
                camHolder.localRotation =
                    Quaternion.Slerp(camHolder.localRotation, finalRot, strength);

                timer -= Time.deltaTime;
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

        private IEnumerator HeadBob()
        {
            
            yield return null;
        }
    }
}