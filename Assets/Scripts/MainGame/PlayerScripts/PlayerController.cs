using System;
using System.Collections;
using System.Collections.Generic;
using MainGame;
using MainGame.PlayerScripts;
using MainGame.PlayerScripts.Roles;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
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
    [Range(-2, 2)] public float backShift = -0.23f;

    // Light
    [SerializeField] private GameObject Lamp;
    
    // Player Controls
    public PlayerControls PlayerControls;

    // Ai settings
    private Role _role;
    private GameObject AiInstance;
    private Transform villageTransform;
    [SerializeField] private GameObject AiPrefab;
    private List<Transform> playerPositions;

    private float minVillageDist = 120f;
    private float minPlayerDist = 60f;
    public bool IaAlreadySpawned => AiInstance;
    private bool hasAlreadySpawnedToday;
    [SerializeField] private bool enableAi = true;
    
    // Sound for Ai
    [SerializeField] private AudioClip aiSound;
    [SerializeField] private AudioSource plyAudioSource;

    private Light _lampLight;
    // [SerializeField] [Range(0f, 1f)] private float slider;

    #endregion

    #region Unity methods

    private void OnEnable()
    {
        playerInput.enabled = true;
    }

    internal void OnDisable()
    {
        playerInput.enabled = false;
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
    }

    internal void Start()
    {
        // Moves the player render backwards so that it doesn't clip with the camera
        var transformLocalPosition = PlayerRender.transform.localPosition;
        transformLocalPosition.z = -backShift;
        PlayerRender.transform.localPosition = transformLocalPosition;

        /*// DEBUG: sets the timescale
        IEnumerator TimeScaler()
        {
            while (true)
            {
                Time.timeScale = slider;
                yield return null;
            }
        }
        StartCoroutine(TimeScaler());*/
        
        _role = GetComponent<Role>();
        
        // Starts the light management
        _lampLight = Lamp.GetComponent<Light>();
        StartCoroutine(LightManager());
        
        // Pre-start Ai
        {
            try
            {
                villageTransform = GameObject.FindWithTag("village").transform;

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
        
        // Starts the Ai
        if (!_photonView.IsMine)
        {
            Destroy(_cam);
            Destroy(_audioListener);
            playerInput.enabled = false;
            enableAi = false;
        }
        else
        {
            if (enableAi)
            {
                StartCoroutine(AiCreator());
            }
        }
    }

    private IEnumerator LightManager()
    {
        // Non-werewolves don't see lights
        if (RoomManager.Instance.localPlayer.GetType() != typeof(Werewolf))
        {
            _lampLight.intensity = 0;
            yield break;
        }

        // Werewolves have a red light, others have a white one
        _lampLight.color = _role.GetType() == typeof(Werewolf) ? Color.red : Color.white;

        while (true)
        {
            // It's day, turn off light
            _lampLight.intensity = 0;
            
            yield return new WaitUntil(() => VoteMenu.Instance.isNight);
            
            // It's night, turn on light
            _lampLight.intensity = 2;
            
            yield return new WaitUntil(() => !VoteMenu.Instance.isNight);
        }
    }

    private IEnumerator AiCreator()
    {
        // Werewolves are not affected
        if (_role.GetType() == typeof(Werewolf)) yield break;
        
        while (true)
        {
            if (CanAiSpawn())
            {
                // Can spawn the Ai
                AiInstance = Instantiate(AiPrefab,
                    transform.position + transform.TransformDirection(Vector3.back * 10 + Vector3.up * 2),
                    Quaternion.identity);
                
                plyAudioSource.Stop();
                plyAudioSource.clip = aiSound;
                plyAudioSource.Play();
                var a = AiInstance.GetComponent<AiController>();
                a.targetRole = _role;
                a._camHolder = cameraHolder;

                hasAlreadySpawnedToday = true;

                Debug.Log("Ai created");
            }

            yield return new WaitForSeconds(2);
        }
    }

    private bool CanAiSpawn()
    {
        // Already spawned today check
        try
        {
            if (!VoteMenu.Instance.isNight)
            {
                hasAlreadySpawnedToday = false;
            }
        }
        catch
        {
            hasAlreadySpawnedToday = false;
        }

        if (hasAlreadySpawnedToday)
        {
            // Debug.Log("SPAWNCHECK (0/5): Already spawn today");
            return false;
        }

        // Already spawned check
        if (IaAlreadySpawned)
        {
            // Debug.Log("SPAWNCHECK (1/5): Ai already exists");
            return false;
        }

        try
        {
            // Alive check
            if (!_role.isAlive)
            {
                // Debug.Log("SPAWNCHECK (2/5): is dead");
                return false;
            }

            // Day check
            if (!VoteMenu.Instance.isNight)
            {
                // Debug.Log("SPAWNCHECK (3/5): it's not night", VoteMenu.Instance.gameObject);
                return false;
            }


            // Village check
            bool villageTooClose = (villageTransform.position - transform.position).sqrMagnitude <
                                   minVillageDist * minVillageDist;
            if (villageTooClose)
            {
                // Debug.Log("SPAWNCHECK (4/5): village is too close");
                return false;
            }

            // Player check
            bool everyPlayerFarEnough = true;
            for (int i = 0; i < playerPositions.Count && everyPlayerFarEnough; i++)
            {
                everyPlayerFarEnough &= (playerPositions[i].position - transform.position).sqrMagnitude >
                                        minPlayerDist * minPlayerDist;
            }

            if (!everyPlayerFarEnough)
            {
                // Debug.Log("SPAWNCHECK (5/5): a player is too close");
                return false;
            }
        }
        catch
        {
            return true;
        }

        return true;
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
    }