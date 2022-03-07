using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
// ReSharper disable All

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

    public PlayerInput PlayerInput => playerInput;

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
    }
    
    private void Start()
    {
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
        _playerAnimation.UpdateAppearance(_playerMovement.currentMovementType);
    }
    
    private void FixedUpdate()
    {
        if (!_photonView.IsMine) return;
        
        //TODO
        // Will soon be improved to remove jittering
        _playerMovement.Move();
        _playerMovement.UpdateHitbox();
    }
    
    #endregion
    

    // Network syncronization
    #region RPCs

    [PunRPC]
    // Syncronizes the appearance
    void RPC_UpdateAppearance(PlayerMovement.MovementTypes movementType) => _playerAnimation.UpdateAppearance(_playerMovement.currentMovementType);

    #endregion
}
