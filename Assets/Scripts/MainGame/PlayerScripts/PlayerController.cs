using System;
using System.Linq;
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
    internal CharacterController characterController;
    
    // Network component
    private PhotonView _photonView;
    
    // Sub scripts
    internal PlayerMovement playerMovement;
    internal PlayerLook playerLook;
    private PlayerAnimation playerAnimation;

    // Miscellaneous
    [SerializeField] internal GameObject cameraHolder;
    internal Camera camera;
    [SerializeField] private PlayerInput playerInput;
    
    // Player Controls
    internal PlayerControls playerControls;

    #endregion

    #region Unity methods
    
    private void Awake()
    {
        // Player Controls
        playerControls = new PlayerControls();

        // Movement components
        characterController = GetComponent<CharacterController>();
        
        // Network component
        _photonView = GetComponent<PhotonView>();
        
        // Camera component
        camera = cameraHolder.GetComponentInChildren<Camera>();

        if (null == camera) throw new Exception("There is no camera attached to the Camera Holder !");
        
        // Sub scripts
        playerMovement = GetComponent<PlayerMovement>();
        playerLook = GetComponent<PlayerLook>();
        playerAnimation = GetComponent<PlayerAnimation>();
    }
    
    private void Start()
    {
        if (_photonView.IsMine) return;
        
        Destroy(cameraHolder);
        Destroy(characterController);
    }

    private void Update()
    {
        if (!_photonView.IsMine) return;
        
        playerLook.Look();
        
        // Updates the jump feature
        playerMovement.UpdateJump();
        
        // Updates the appearance based on the MovementType
        playerAnimation.UpdateAppearance();
    }
    
    private void FixedUpdate()
    {
        if (!_photonView.IsMine) return;
        
        //TODO improve to remove jittering
        playerMovement.Move();
        
        playerMovement.UpdateHitbox();
    }
    
    #endregion
    

    // Network synchronization
    #region RPCs

    [PunRPC]
    // Synchronizes the appearance
    void RPC_UpdateAppearance(PlayerMovement.MovementTypes movementType)
    {
        playerAnimation.UpdateAppearance();
    }

    #endregion
}
