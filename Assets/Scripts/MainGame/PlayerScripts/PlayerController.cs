using System;
using MainGame.PlayerScripts.Roles;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement)),
 RequireComponent(typeof(PlayerLook)),
 RequireComponent(typeof(PlayerAnimation))]

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
    private PlayerAnimation _playerAnimation;

    // Miscellaneous
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] internal GameObject cameraHolder;
    private Camera _cam;
    
    // Player Controls
    public PlayerControls PlayerControls;

    #endregion

    #region Unity methods
    
    private void Awake()
    {
        // Player Controls
        PlayerControls = new PlayerControls();

        // Movement components
        _characterController = GetComponent<CharacterController>();
        
        // Network component
        _photonView = GetComponent<PhotonView>();
        
        // Camera component
        _cam = cameraHolder.GetComponentInChildren<Camera>();

        if (null == _cam) throw new Exception("There is no camera attached to the Camera Holder !");
        
        // Sub scripts
        _playerMovement = GetComponent<PlayerMovement>();
        _playerLook = GetComponent<PlayerLook>();
        _playerAnimation = GetComponent<PlayerAnimation>();
    }
    
    internal void Start()
    {
        if (_photonView.IsMine) return;
        
        Destroy(cameraHolder);
        // Destroy(_characterController);
        playerInput.enabled = false;
    }

    private void Update()
    {
        if (!_photonView.IsMine) return;
        
        _playerLook.Look();
        
        // Updates the jump feature
        _playerMovement.UpdateJump();
        
        // Updates the appearance based on the MovementType
        _playerAnimation.UpdateAppearance();
    }
    
    private void FixedUpdate()
    {
        if (!_photonView.IsMine) return;
        
        //TODO improve to remove jitter
        _playerMovement.Move();
        
        _playerMovement.UpdateHitbox();
    }
    
    #endregion
    

    // Network synchronization
    #region RPCs

    [PunRPC]
    // Synchronizes the appearance
    void RPC_UpdateAppearance(PlayerMovement.MovementTypes movementType)
    {
        _playerAnimation.UpdateAppearance();
    }

    #endregion
}
