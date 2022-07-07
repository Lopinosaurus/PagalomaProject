using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

namespace MainGame.PlayerScripts
{
    public class PlayerLook : MonoBehaviour
    {
        #region Attributes

        // Components
        private Camera _cam;
        private Transform _camTransform;
        private PlayerInput _playerInput;
        private RotationConstraint _rotationConstraint;
        private PhotonView _photonView;
        private FootstepEffect _footstepEffect;
        private PlayerAnimation _playerAnimation;
        private DepthOfField _depthOfField;
        private CharacterController _characterController;
        [SerializeField] private Transform camHolder;

        // Sensitivity
        [Space, Header("Mouse settings")] private const float MouseSensX = 10, MouseSensY = 10;
        private float _maxLookClamp = -70, _minLookClamp = 80;

        public void ClampView(PlayerMovement.MovementState movementState)
        {
            _minLookClamp = movementState switch
            {
                PlayerMovement.MovementState.Crouch => 60,
                _ => 80
            };
        }
        
        public void ClampView(PlayerMovement.JumpState jumpState)
        {
            _minLookClamp = jumpState switch
            {
                PlayerMovement.JumpState.MidVault => 60,
                _ => 80
            };
        }
        
        // Mouse input values
        private float _mouseDeltaX, _mouseDeltaY;
        private float CanTurnSidesMult => !_canTurnSides ? 0.1f : 1;

        // Current player values
        private float _rotationSidesX, _rotationVerticalY;
        private Quaternion _targetCamHolderRot;
        private Quaternion _targetBodyPlayerRot;
        [SerializeField, Range(0.01f, 1)] private float smoothTimeCam = 0.01f;
        
        private enum CamState
        {
            Normal,
            Shaking
        }

        private CamState _currentState = CamState.Normal;

        // Shake settings
        [Space, Header("Shake settings")]
        [SerializeField, Range(0, 30)] private float screenShakeLerp;

        // Jump settings
        private bool _canTurnSides = true;

        // Head settings
        [Space, Header("Head rotation settings")]
        [SerializeField, Range(0, 90)] private float headAngleRotThreshold = 60f;
        [SerializeField] private Transform bodyToRotate;
        private float _deltaRotation;
        private float _rotationRef;
        private bool _isMoving;
        private const float BodyResetRotStrength = 12;

        // Head Bob settings
        [Space, Header("HeadBob settings")]
        [SerializeField, Range(0, 1)] private float amplitude = 0.1f;
        [SerializeField, Range(0, 30)] private float headBobLerp;

        // FOV
        [Space, Header("Dynamic FOV settings")]
        [SerializeField, Range(0, 5)] private float addMovementFOV = 2;
        private float _baseFOV;

        public void HeadBob()
        {
            if (CamState.Normal != _currentState) return;
            
            Vector3 camPosRef = _camTransform.localPosition;
            Vector3 camPosNew = camPosRef;

            camPosNew.y = amplitude * Mathf.Clamp01(Mathf.PerlinNoise(Time.time, 0) - .5f);
            camPosNew.x = amplitude * (Mathf.Clamp01(Mathf.PerlinNoise(0, Time.time)) - .5f);
            
            _camTransform.localPosition = Vector3.Slerp(camPosRef, camPosNew, Time.deltaTime * headBobLerp);
        }

        #endregion
        
        public void Look() // Modifies camera and player rotation
        {
            _rotationVerticalY -= _mouseDeltaY * MouseSensY * CanTurnSidesMult;
            _rotationSidesX += _mouseDeltaX * MouseSensX * CanTurnSidesMult;

            _rotationVerticalY = Mathf.Clamp(_rotationVerticalY, _maxLookClamp, _minLookClamp);
            Quaternion playerLocalRotation = transform.localRotation;

            // Only modifies the desired axis (X)
            _targetCamHolderRot = camHolder.transform.localRotation;
            Vector3 euler = _targetCamHolderRot.eulerAngles;
            euler.x = Quaternion.AngleAxis(_rotationVerticalY, Vector3.right).eulerAngles.x;
            _targetCamHolderRot.eulerAngles = euler;

            // Only modifies the desired axis (Y)
            _targetBodyPlayerRot = playerLocalRotation;
            euler = _targetBodyPlayerRot.eulerAngles;
            euler.y = Quaternion.AngleAxis(_rotationSidesX, Vector3.up).eulerAngles.y;
            _targetBodyPlayerRot.eulerAngles = euler;
            
            camHolder.localRotation = Quaternion.Lerp(camHolder.localRotation, _targetCamHolderRot, smoothTimeCam);
            transform.localRotation = Quaternion.Lerp(playerLocalRotation, _targetBodyPlayerRot, smoothTimeCam);
        }

        public void StartShake(float duration, float shakeStrength, float shakeAmplitude)
        {
            _currentState = CamState.Shaking;
            
            shakeStrength = shakeStrength < 0 ? 0 : shakeStrength * 300;
            StartCoroutine(Shake(duration, shakeStrength, shakeAmplitude));
        }

        private IEnumerator Shake(float duration, float shakeStrength, float shakeAmplitude)
        {
            float timer = 0;

            // Twist angle
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float currentStrength = Mathf.SmoothStep(shakeStrength, 0, timer / duration);

                Vector3 camPosNew = _camTransform.localPosition;

                camPosNew.y = shakeAmplitude * (Mathf.Clamp01(Mathf.PerlinNoise(timer * currentStrength, 0)) - 0.5f);
                camPosNew.x = shakeAmplitude * (Mathf.Clamp01(Mathf.PerlinNoise(0, timer * currentStrength)) - 0.5f);

                _camTransform.localPosition = camPosNew;
                
                yield return null;
            }

            // Restore normal angle
            ResetCam();

            _currentState = CamState.Normal;
        }

        private void ResetCam()
        {
            // Brings the camera back to the camHolder position
            _camTransform.localPosition = Vector3.zero;
        }

        public void HeadRotate()
        {
            // X-axis applies at all times (component)

            // Y-axis applies when the player stops moving
            _isMoving = Mathf.Abs(_playerAnimation.VelocityX) > 0.1f || Mathf.Abs(_playerAnimation.VelocityZ) > 0.1f;

            if (_isMoving)
            {
                _rotationRef = transform.rotation.eulerAngles.y;
                RotateBodyY(0);
            }
            else
            {
                // This delta is the difference between the last rotation while moving and the current rotation
                float rotation = transform.rotation.eulerAngles.y;
                _deltaRotation = Mathf.DeltaAngle(rotation, _rotationRef);

                // If the body rotates beyond that limit, we re-adapt
                if (Mathf.Abs(_deltaRotation) > headAngleRotThreshold)
                {
                    float min = rotation - headAngleRotThreshold;
                    float max = rotation + headAngleRotThreshold;

                    if (_rotationRef > max && _deltaRotation < 0) _rotationRef -= 360;
                    if (_rotationRef < min && _deltaRotation > 0) _rotationRef += 360;


                    _rotationRef = Mathf.Clamp(_rotationRef, min, max);
                }
                else
                {
                    RotateBodyY(_deltaRotation, true);
                }
            }
        }

        private void RotateBodyY(float rotY, bool isInstant = false)
        {
            Vector3 localRotHips = bodyToRotate.localRotation.eulerAngles;
            float resetRotStrength = isInstant ? 1 : BodyResetRotStrength * Time.deltaTime;
            localRotHips.y = Mathf.LerpAngle(localRotHips.y, rotY, resetRotStrength);

            bodyToRotate.transform.localRotation = Quaternion.Euler(localRotHips);
        }

        public void LockViewJump(bool locked)
        {
            _canTurnSides = !locked;
        }

        public void LocalPostProcessingAndSound(PostProcessVolume postProcessVolume, float duration,
            AudioClip audioClip)
        {
            /*//TODO Change
            
            // Attach gameObject to player
            GameObject localVolume = new GameObject($"localVolume ({duration})");
            localVolume.transform.SetParent(transform);

            // Attach spawner
            FXManager.Instance.CreateAudioFX();
            spawner.enabled = true;

            // Attach postprocess
            PostProcessVolume processVolume = _cam.gameObject.AddComponent<PostProcessVolume>();
            postProcessVolume.weight = 1;
            postProcessVolume.priority = 1;
            postProcessVolume.isGlobal = false;

            // Attach and execute sound if available
            if (audioClip)
            {
                AudioSource audioSource = _cam.gameObject.AddComponent<AudioSource>();
                audioSource.clip = audioClip;
                audioSource.Play();
                
                // Destroy with timer
                Destroy(audioSource, duration);
            }
            
            // Destroy with timer
            Destroy(processVolume, duration);*/
        }

        public void FOVChanger()
        {
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, _baseFOV + addMovementFOV * _playerAnimation.Velocity, 1);
        }

        public void DofChanger()
        {
            float focusDistanceValue = _depthOfField.focusDistance.value;
            float newFocusDistanceValue = focusDistanceValue;

            Transform camTransform = _cam.transform;

            if (Physics.SphereCast(camTransform.position, 0.01f, camTransform.forward, out RaycastHit hit,
                    float.PositiveInfinity, 7, QueryTriggerInteraction.Ignore))
                newFocusDistanceValue = hit.distance;

            _depthOfField.focusDistance.value = newFocusDistanceValue;

        }
        
        #region Unity Methods

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerInput = GetComponent<PlayerInput>();
            _playerAnimation = GetComponent<PlayerAnimation>();
            _depthOfField = GetComponentInChildren<PostProcessVolume>().profile.GetSetting<DepthOfField>();
            _photonView = GetComponent<PhotonView>();
            _footstepEffect = GetComponent<FootstepEffect>();
            _cam = GetComponentInChildren<Camera>();
            _camTransform = _cam.transform;
            _rotationConstraint = GetComponentInChildren<RotationConstraint>();

            // FOV
            _baseFOV = _cam.fieldOfView;

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
    }
}