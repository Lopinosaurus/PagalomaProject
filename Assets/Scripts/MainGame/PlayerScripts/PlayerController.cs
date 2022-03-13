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
    private CharacterController characterController;
    
    // Network component
    private PhotonView _photonView;
    
    // Sub scripts
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerAnimation playerAnimation;

    // Miscellaneous
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] internal GameObject cameraHolder;
    private Camera cam;
    
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
        cam = cameraHolder.GetComponentInChildren<Camera>();

        if (null == cam) throw new Exception("There is no camera attached to the Camera Holder !");
        
        // Sub scripts
        playerMovement = GetComponent<PlayerMovement>();
        playerLook = GetComponent<PlayerLook>();
        playerAnimation = GetComponent<PlayerAnimation>();
    }
    
    internal void Start()
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
