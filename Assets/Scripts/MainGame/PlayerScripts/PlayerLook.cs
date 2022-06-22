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
        public void Look() // Modifies camera and player rotation
        {
            rotationY = _mouseDeltaX * _mouseSensX * CanTurnSidesMult;
            _rotationX -= _mouseDeltaY * _mouseSensY * CanTurnSidesMult;

            _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);

            var _ = 0f;
            if (_rotationX < -70f)
                _rotationX = Mathf.SmoothDampAngle(_rotationX, -70f, ref _, SmoothTimeX);
            else if (_rotationX > 80f) _rotationX = Mathf.SmoothDampAngle(_rotationX, 80f, ref _, SmoothTimeX);

            Vector3 localRotationEuler = camHolder.localRotation.eulerAngles;
            localRotationEuler.x = _rotationX;
            camHolder.localRotation = Quaternion.Euler(localRotationEuler);

            // _characterController.transform.rotation = Quaternion.Euler(rotationEuler);
            _characterController.transform.Rotate(Vector3.up, rotationY);
        }

        public void StartShake(float duration, float shakeStrength)
        {
            shakeStrength = shakeStrength < 0 ? 0 : shakeStrength * 300;
            StartCoroutine(Shake(duration, shakeStrength));
        }

        private IEnumerator Shake(float duration, float shakeStrength)
        {
            float timer = 0;
            int side = Random.Range(0, 2) == 1 ? 1 : -1;
            float angleSoFar = 60;

            // Twist angle
            while (timer < duration)
            {
                timer += Time.deltaTime;

                float strength = (duration - timer) / duration;
                float deltaAngle = shakeStrength * strength * side * Time.deltaTime;

                float absDeltaAngle = Mathf.Abs(deltaAngle);
                angleSoFar += absDeltaAngle;

                if (angleSoFar >= 180)
                {
                    angleSoFar = 0;
                    side *= -1;
                }

                Vector3 forwardCamHolder = camHolder.InverseTransformDirection(camHolder.forward);

                camHolder.Rotate(forwardCamHolder, deltaAngle);
                Vector3 localRotationEulerAngles = camHolder.localRotation.eulerAngles;
                localRotationEulerAngles.y = 0;
                camHolder.localRotation = Quaternion.Euler(localRotationEulerAngles);

                yield return null;
            }

            Vector3 finalRotEulerAngles = new(camHolder.localRotation.eulerAngles.x, 0, 0);
            Quaternion finalRot = Quaternion.Euler(finalRotEulerAngles);
            // Restore normal angle
            timer = 10;
            while (Mathf.Abs(Quaternion.Angle(camHolder.localRotation, finalRot)) != 0 && timer > 0)
            {
                // Final values
                Quaternion localRotation = camHolder.localRotation;
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
            _isMoving = Mathf.Abs(_playerAnimation.VelocityX) > 0.1f ||
                       Mathf.Abs(_playerAnimation.VelocityZ) > 0.1f;

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
                if (Mathf.Abs(_deltaRotation) > rotationThreshold)
                {
                    float min = rotation - rotationThreshold;
                    float max = rotation + rotationThreshold;

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
            // Attach gameObject to player
            GameObject localVolume = new($"localVolume ({duration})");
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

            // Execute sound if available
            if (audioClip != null)
            {
                AudioSource audioSource = localVolume.AddComponent<AudioSource>();
                audioSource.clip = audioClip;
                audioSource.Play();
            }
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

        #region Attributes

        // Components
        [SerializeField] private Transform camHolder;
        private Camera _cam;
        private PlayerInput _playerInput;
        private RotationConstraint _rotationConstraint;
        private PhotonView _photonView;
        private FootstepEffect _footstepEffect;
        private PlayerAnimation _playerAnimation;
        private DepthOfField _depthOfField;
        private CharacterController _characterController;

        // Sensitivity
        [Space] [Header("Mouse settings")] private readonly float _mouseSensX = 10f;

        private readonly float _mouseSensY = 10f;

        // Mouse input values
        private float _mouseDeltaX;
        private float _mouseDeltaY;
        public float TurnDeltaY => _mouseDeltaX * _mouseSensX * CanTurnSidesMult;
        private float CanTurnSidesMult => !_canTurnSides ? 0.1f : 1;

        // Current player values
        private float _rotationX;
        public float rotationY;
        private const float SmoothTimeX = 0.01f;

        // Shake settings
        [Space] [Header("Shake settings")]

        // Jump settings
        private bool _canTurnSides = true;

        // Head settings
        [SerializeField] private Transform bodyToRotate;
        private float _deltaRotation;
        private float _rotationRef;
        private const float BodyResetRotStrength = 12;
        [SerializeField] [Range(0, 90)] private float rotationThreshold = 60f;
        private bool _isMoving;

        // Head Bob settings
        private const float Amplitude = 0.1f;
        private float _piDividedByMaxDistance;
        private const float PIHalf = Mathf.PI * 0.5f;

        // FOV
        private float _baseFOV;
        [SerializeField] private float addMovementFOV = 2;
        
        public void HeadBob()
        {
            Vector3 pos = camHolder.position;

            pos.y -= Mathf.Cos(_footstepEffect.PlayerDistanceCounter * _piDividedByMaxDistance + PIHalf) * Amplitude;
            Transform camTransform = _cam.transform;
            camTransform.position = pos;
            Vector3 localPosition = camTransform.localPosition;
            localPosition.z = 0;

            camTransform.localPosition = localPosition;
        }

        #endregion

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
            _rotationConstraint = GetComponentInChildren<RotationConstraint>();

            // HeadBob
            _piDividedByMaxDistance = Mathf.PI / _footstepEffect.MaxDistance;

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