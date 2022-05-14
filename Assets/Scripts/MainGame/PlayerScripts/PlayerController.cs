using System;
<<<<<<< Updated upstream
using UnityEngine;
using Photon.Pun;
=======
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
>>>>>>> Stashed changes
using UnityEngine.InputSystem;

namespace MainGame.PlayerScripts
{
<<<<<<< Updated upstream
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
        
=======
    [RequireComponent(typeof(PlayerMovement)),
     RequireComponent(typeof(PlayerLook))]
    public class PlayerController : MonoBehaviour
    {
        #region Attributes

>>>>>>> Stashed changes
        // Network component
        private PhotonView _photonView;

        // Sub scripts
<<<<<<< Updated upstream
        _playerMovement = GetComponent<PlayerMovement>();
        _playerLook = GetComponent<PlayerLook>();
    }
    
    internal void Start()
    {
        if (!_photonView.IsMine)
=======
        private PlayerMovement _playerMovement;
        private PlayerLook _playerLook;
        private Role _role;
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

        // Ai settings
        public bool willSpawnAi = false;
        public GameObject AiPrefab;
        private Transform VillageTransform;
        private float minVillageDistance = 60;
        private float minPlayerDistance = 60;
        private List<Role> playerRoles;

        #endregion

        #region Unity methods

        private void OnEnable() => PlayerControls.Player.Enable();

        private void OnDisable() => PlayerControls.Player.Disable();

        private void Awake()
>>>>>>> Stashed changes
        {
            // Player Controls
            PlayerControls = new PlayerControls();

            // Network component
            _photonView = GetComponent<PhotonView>();

            // Camera component
            _cam = cameraHolder.GetComponentInChildren<Camera>();
            _audioListener = cameraHolder.GetComponentInChildren<AudioListener>();

            if (_cam == null) throw new Exception("There is no camera attached to the Camera Holder !");

            // Sub scripts
            _playerMovement = GetComponent<PlayerMovement>();
            _playerLook = GetComponent<PlayerLook>();
            _role = GetComponent<Role>();

            // Ai components
            if (willSpawnAi)
            {
                try
                {
                    RoomManager roomManager = FindObjectOfType<RoomManager>();
                    foreach (var role in roomManager.players) playerRoles.Add(role);
                    try
                    {
                        playerRoles.Remove(_role);
                    }
                    catch
                    {
                        Debug.LogWarning("Couldn't delete self from player list, should never happen");
                    }
                }
                catch (Exception)
                {
                    Debug.LogWarning("RoomManager not found");
                }

                try
                {
                    VillageTransform = GameObject.FindWithTag("village").transform;
                }
                catch (Exception)
                {
                    Debug.LogWarning("Village not found");
                }
            }
        }

        internal void Start()
        {
<<<<<<< Updated upstream
            _playerLook.Look();

            // Updates the jump feature
            _playerMovement.UpdateJump();
            
            // Updates the appearance based on the MovementType
            _playerAnimation.UpdateAnimationsBasic();
=======
            // Moves the player render backwards so that it doesn't clip with the camera
            PlayerRender.transform.localPosition -= Vector3.back * backShift;

            if (!_photonView.IsMine)
            {
                Destroy(_cam);
                Destroy(_audioListener);
                playerInput.enabled = false;
            }

            if (willSpawnAi)
            {
                StartCoroutine(nameof(CreateAi));
            }
>>>>>>> Stashed changes
        }

        private IEnumerator CreateAi()
        {
<<<<<<< Updated upstream
            //TODO improve to remove jitter
            _playerMovement.Move();

            _playerMovement.UpdateHitbox();


=======
            // Skips if there is no village
            if (VillageTransform)
            {
                // Checks whether village if far enough
                if ((VillageTransform.position - transform.position).sqrMagnitude <
                    minVillageDistance * minVillageDistance)
                {
                    yield return new WaitForSeconds(2);
                }
            }

            yield return CheckForClosestPlayer();

            Transform transform1 = transform;
            var spawn = transform1.position + transform.TransformDirection(Vector3.back * 5);
            GameObject ai = Instantiate(AiPrefab, spawn, Quaternion.identity);

            // AiController set up
            AiController _aiController = ai.GetComponent<AiController>();
            _aiController.targetRole = _role;
            _aiController.enabled = true;

            yield return null;
>>>>>>> Stashed changes
        }

        private IEnumerator CheckForClosestPlayer()
        {
            bool allFarEnough = false;
            int index = 0;

            if (playerRoles != null && playerRoles.Count > 1)
            {
                while (!allFarEnough)
                {
                    allFarEnough = true;

                    // Checks for distance
                    if ((transform.position - playerRoles[index].transform.position).sqrMagnitude <
                        minPlayerDistance * minPlayerDistance)
                    {
                        allFarEnough = false;
                        index = 0;
                    }

                    yield return new WaitForSeconds(2);
                }
            }

            yield return null;
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
}