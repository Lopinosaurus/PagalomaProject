using System;
using System.Collections;
using MainGame.PlayerScripts.Roles;
using UnityEngine;
using Photon.Pun;
// ReSharper disable All

[RequireComponent(typeof(PlayerMovement)),
 RequireComponent(typeof(PlayerLook)),
 RequireComponent(typeof(PlayerAnimation)),
 RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    #region Attributes
    
    // Movement components
    internal CharacterController _characterController;
    
    // Network component
    private PhotonView _photonView;
    
    // Sub scripts
<<<<<<< Updated upstream
    private PlayerMovement _playerMovement;
    private PlayerLook _playerLook;
    private PlayerAnimation _playerAnimation;
    
    // Miscellaneous
=======
    internal PlayerMovement _playerMovement;
    internal PlayerLook _playerLook;
    internal PlayerAnimation _playerAnimation;

    // Miscellaneous
    [SerializeField] internal GameObject cameraHolder;
    [SerializeField] public LayerMask groundMask;

    [SerializeField] private PlayerInput playerInput;
    public PlayerInput PlayerInput => playerInput;
    internal Camera _camera;
>>>>>>> Stashed changes

    #endregion

    #region Unity methods
    
    private void Awake()
    {
        // Movement components
        _characterController = GetComponent<CharacterController>();
        
        // Network component
        _photonView = GetComponent<PhotonView>();
        
        // Sub scripts
        _playerMovement = GetComponent<PlayerMovement>();
        _playerLook = GetComponent<PlayerLook>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        
        // Miscellaneous
        _camera = cameraHolder.GetComponentInChildren<Camera>();
        if (_camera == null)
        {
            throw new Exception("There is no camera attached to the CamHolder child !");
        }
    }
    
    private void Start()
    {
        Time.timeScale = 1f; // should be removed if necessary
        
        if (_photonView.IsMine) return;
        
        Destroy(GetComponentInChildren<Camera>().gameObject);
        Destroy(_characterController);
    }

    private void Update()
    {
        if (!_photonView.IsMine) return;
        
        _playerLook.Look();
        
        // Updates the jump feature
        _playerMovement.UpdateJump();
        
        // Updates the appearance based on the MovementType
<<<<<<< Updated upstream
        _playerAnimation.UpdateAppearance(_playerMovement.currentMovementType);
=======
        _playerAnimation.UpdateMovementAppearance();
>>>>>>> Stashed changes
    }
    
    private void FixedUpdate()
    {
        if (!_photonView.IsMine) return;
        
        //TODO
        // Will soon be improved to remove jittering
        _playerMovement.Move();
<<<<<<< Updated upstream

        UpdateHitbox();
    }
    
    #endregion

    private void UpdateHitbox() => _playerAnimation.UpdateHitbox(_playerMovement.currentMovementType);


    // Network syncronization
    #region RPCs

    [PunRPC]
    // Syncronizes the appearance
    void RPC_UpdateAppearance(PlayerMovement.MovementTypes movementType) => _playerAnimation.UpdateAppearance(_playerMovement.currentMovementType);
=======
        
        _playerMovement.UpdateHitbox();
    }
    
    #endregion
    
    // Network synchronization
    #region RPCs

    [PunRPC]
    // Synchronizes the appearance
    void RPC_UpdateAppearance(PlayerMovement.MovementTypes movementType)
    {
        _playerAnimation.UpdateMovementAppearance();
    }
>>>>>>> Stashed changes

    #endregion
}
