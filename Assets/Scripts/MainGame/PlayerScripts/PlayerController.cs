using System;
using System.Collections;
using System.Collections.Generic;
using MainGame;
using MainGame.PlayerScripts;
using MainGame.PlayerScripts.Roles;
using UnityEngine;
using Photon.Pun;
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

    // Ai settings
    private Role _role;
    private GameObject AiInstance;
    private Transform villageTransform;
    [SerializeField] private GameObject AiPrefab;
    private List<Transform> playerPositions;
    
    private float minVillageDist = 60f;
    private float minPlayerDist = 60f;
    private bool IaAlreadySpawned => AiInstance;

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

        // Ai
        _role = GetComponent<Role>();

        try
        {
            villageTransform = RoomManager.Instance.map.village.transform;
            
            var t = RoomManager.Instance.players;
            playerPositions = new List<Transform>();
            foreach (var role in t)
            {
                if (role.userId != _role.userId)
                {
                    playerPositions.Add(role.gameObject.transform);
                }
            }
        }
        catch
        {
            Debug.LogWarning("No RoomManager found ! (PlayerController)");
        }
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
        else
        {
            StartCoroutine(AiCreator());
        }
    }

    private IEnumerator AiCreator()
    {
        while (true)
        {
            while (true)
            {
                yield return new WaitForSeconds(2);

                // Already spawned check
                if (IaAlreadySpawned) continue;
                Debug.Log("SPAWNCHECK (1/5): No other exists");

                try
                {
                    // Alive check
                    if (!_role.isAlive) continue;
                    Debug.Log("SPAWNCHECK (2/5): is alive");

                    // Day check
                    if (VoteMenu.Instance.isDay) continue;
                    Debug.Log("SPAWNCHECK (3/5): is not day", VoteMenu.Instance);

                    // Village check
                    Debug.Log($"villagePos = {villageTransform.position} | pos = {transform.position}");
                    bool villageTooClose =(villageTransform.position - transform.position).sqrMagnitude <
                                          minVillageDist * minVillageDist;
                    if (villageTooClose) continue;
                    Debug.Log("SPAWNCHECK (4/5): village is far enough");

                    // Player check
                    bool everyPlayerFarEnough = true;
                    for (int i = 0; i < playerPositions.Count && everyPlayerFarEnough; i++)
                    {
                        everyPlayerFarEnough &= (playerPositions[i].position - transform.position).sqrMagnitude >
                                               minPlayerDist * minPlayerDist;
                    }

                    if (!everyPlayerFarEnough) continue;
                    Debug.Log("SPAWNCHECK (5/5): every player is far enough");

                }
                catch
                {
                    Debug.LogWarning("No RoomManager found ! (PlayerController)");
                }

                
                // All conditions are valid
                break;
            }

            // Can spawn the Ai
            AiInstance = Instantiate(AiPrefab, 
                transform.position + transform.TransformDirection(Vector3.back + Vector3.up), Quaternion.identity);

            AiInstance.GetComponent<AiController>().targetRole = _role;
            
            Debug.Log("Ai created");

            yield return null;
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