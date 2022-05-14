using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MainGame.PlayerScripts
{
    public class PlayerLook : MonoBehaviour
    {
        #region Attributes

        [SerializeField] private Transform camHolder;
        private PlayerInput _playerInput;
        private PhotonView _photonView;
        private CharacterController _characterController;
    
        // Sensitivity
        [Space]
        [Header("Mouse settings")]
        [Range(4f, 128f)]
        [SerializeField] private float mouseSensX = 10f;
        [Range(4f, 128f)]
        [SerializeField] private float mouseSensY = 10f;

<<<<<<< Updated upstream
    // private float YLookRotation;
    private bool shouldLookAround = true;

    // Mouse input values
    private float _mouseDeltaX;
    private float _mouseDeltaY;

    // Current player values
    private float _rotationX;
    private float _rotationY;
    private float _smoothValueX;
    private float _smoothValueY;
    private const float SmoothTimeX = 0.01f;
    private const float SmoothTimeY = 0.01f;
=======
        // Mouse input values
        private float _mouseDeltaX;
        private float _mouseDeltaY;

        // Current player values
        private float _rotationX;
        private float _rotationY;
        private const float SmoothTimeX = 0.01f;
>>>>>>> Stashed changes
    
        #endregion

        #region Unity Methods

        private void Awake()
        {
            GetComponent<PlayerController>();
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

            _playerInput.actions["Look"].performed += ctx => _mouseDeltaX = ctx.ReadValue<Vector2>().x;
            _playerInput.actions["Look"].performed += ctx => _mouseDeltaY = ctx.ReadValue<Vector2>().y;
            _playerInput.actions["Look"].canceled += ctx => _mouseDeltaX = ctx.ReadValue<Vector2>().x;
            _playerInput.actions["Look"].canceled += ctx => _mouseDeltaY = ctx.ReadValue<Vector2>().y;
        }


        #endregion
    
<<<<<<< Updated upstream
    public void Look() // Modifies camera and player rotation
    {
        if (!shouldLookAround) return;

        _rotationY += _mouseDeltaX * mouseSensX;
        _rotationX -= _mouseDeltaY * mouseSensY;
=======
        public void Look() // Modifies camera and player rotation
        {
            _rotationY += _mouseDeltaX * mouseSensX;
            _rotationX -= _mouseDeltaY * mouseSensY;
>>>>>>> Stashed changes
        
            _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);

<<<<<<< Updated upstream
        float _ = 0f;
        if (_rotationX < -70f)
        {
            _rotationX = Mathf.SmoothDampAngle(_rotationX, -70f, ref _, SmoothTimeX);
        }
        else if (_rotationX > 80f)
        {
            _rotationX = Mathf.SmoothDampAngle(_rotationX, 80f, ref _, SmoothTimeX);
        }

        Quaternion rotation = Quaternion.Euler(_rotationX, _rotationY, 0);

        _rotationX = Mathf.SmoothDampAngle(_rotationX, rotation.eulerAngles.x, ref _smoothValueX, SmoothTimeX);
        _rotationY = Mathf.SmoothDampAngle(_rotationY, rotation.eulerAngles.y, ref _smoothValueY, SmoothTimeY);

        camHolder.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        _characterController.transform.rotation = Quaternion.Euler(0, _rotationY, 0);

        // transform.Rotate(Vector3.up * (Input.GetAxisRaw("Mouse X") * mouseSensY));
        //
        // YLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensX;
        // YLookRotation = Mathf.Clamp(YLookRotation, -70f, 80f);
        //
        // transform.localEulerAngles = Vector3.left * YLookRotation;
=======
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

            camHolder.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            _characterController.transform.rotation = Quaternion.Euler(0, _rotationY, 0);
        }

        public void NauseaCam(int i)
        {
            throw new System.NotImplementedException();
        }
>>>>>>> Stashed changes
    }
}
