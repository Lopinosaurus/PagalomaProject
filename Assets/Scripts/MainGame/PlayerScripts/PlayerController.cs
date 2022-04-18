using System;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement)),
 RequireComponent(typeof(PlayerLook))]
public class PlayerController : MonoBehaviour
{
    #region Attributes
    
    // Movement components
    private CharacterController _characterController;
    
    // Network component
    private PhotonView _photonView;
    
    // Sub scripts
    private PlayerMovement _playerMovement;
    private PlayerLook _playerLook;
    [SerializeField] private PlayerAnimation _playerAnimation;

    // Miscellaneous
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] internal GameObject cameraHolder;
    private Camera _cam;
    private AudioListener _audioListener;
    
    // Player Controls
    public PlayerControls PlayerControls;

    #endregion

    #region Unity methods
    
    private void OnEnable()
    {
        PlayerControls.Player.Enable();
    }
    
    private void OnDisable()
    {
        PlayerControls.Player.Disable();
    }
    
    private void Awake()
    {
        // Player Controls
        PlayerControls = new PlayerControls();

        // Movement components
        _characterController = GetComponentInChildren<CharacterController>();
        
        // Network component
        _photonView = GetComponent<PhotonView>();
        
        // Camera component
        _cam = cameraHolder.GetComponentInChildren<Camera>();
        _audioListener = cameraHolder.GetComponentInChildren<AudioListener>();

        if (null == _cam) throw new Exception("There is no camera attached to the Camera Holder !");
        
        // Sub scripts
        _playerMovement = GetComponent<PlayerMovement>();
        _playerLook = GetComponent<PlayerLook>();
    }
    
    internal void Start()
    {
        if (!_photonView.IsMine)
        {
            Destroy(_cam);
            Destroy(_audioListener);
            playerInput.enabled = false;
        }
    }

    private void Update()
    {
        if (_photonView.IsMine)
        {
            _playerLook.Look();

            // Updates the jump feature
            _playerMovement.UpdateJump();
            
            // Updates the appearance based on the MovementType
            _playerAnimation.UpdateAnimationsBasic();
        }
    }
    
    private void FixedUpdate()
    {
        if (_photonView.IsMine)
        {
            //TODO improve to remove jitter
            _playerMovement.Move();

            _playerMovement.UpdateHitbox();


        }
    }
    
    #endregion
    

    // Network synchronization
    #region RPCs

    [PunRPC]
    // Synchronizes the appearance
    void RPC_UpdateAppearance(PlayerMovement.MovementTypes movementType)
    {
        _playerAnimation.UpdateAnimationsBasic();
    }

    #endregion
}
