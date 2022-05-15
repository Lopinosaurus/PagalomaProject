using System;
using MainGame.PlayerScripts;
using UnityEngine;
using Photon.Pun;
using UnityEditor;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement)),
 RequireComponent(typeof(PlayerLook))]
public class PlayerController : MonoBehaviour
{
    #region Attributes
    
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
    
    // First person management
    [SerializeField] private GameObject PlayerRender;
    [Range(0, 2)] public float backShift = 1f;

    
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
        // Bake occlusion
        // StaticOcclusionCulling.GenerateInBackground();
        
        // Player Controls
        PlayerControls = new PlayerControls();

        // Movement components
        GetComponentInChildren<CharacterController>();
        
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
        // Moves the player render backwards so that it doesn't clip with the camera
        PlayerRender.transform.localPosition -= Vector3.back * backShift;
        
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
            
            // Moves the player
            _playerMovement.Move(Time.deltaTime);
            
            // Updates the appearance based on the MovementType
            _playerAnimation.UpdateAnimationsBasic();
        }
    }
    
    private void FixedUpdate()
    {
        if (_photonView.IsMine)
        {
            // Adjusts the player's hitbox when crouching
            // TODO fix the tiny gap between the ground and the CharacterController
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
