using System;
using System.Collections;
using System.Collections.Generic;
using MainGame;
using MainGame.PlayerScripts;
using MainGame.PlayerScripts.Roles;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerLook))]
[RequireComponent(typeof(PlayerAnimation))]
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
    [Space, Header("Scripts")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] internal GameObject cameraHolder;
    
    // First person management
    [Space, Header("Camera Components")]
    [SerializeField] private Camera _camPlayer;
    private PostProcessLayer[] _postProcLayers;
    private AudioListener _audioListener;
    [Header("Model Renders")]
    [SerializeField] public GameObject VillagerRender;
    [SerializeField] public GameObject WerewolfRender;
    public readonly float backShift = 0.3f;

    // Ai settings
    [Space, Header("Ai Settings")]
    public Role _role;
    private GameObject AiInstance;
    private Transform villageTransform;
    [SerializeField] private GameObject AiPrefab;
    private List<Transform> playerPositions = new List<Transform>();

    private readonly float minVillageDist = 120f;
    private readonly float minPlayerDist = 60f;
    private bool IaAlreadySpawned => null != AiInstance;
    private bool hasAlreadySpawnedTonight;
    [SerializeField] private bool enableAi = true;

    // Sound for Ai
    [SerializeField] private AudioClip aiSound;
    [SerializeField] private AudioSource playerAudioSource;

    [Space, Header("Light")]
    [SerializeField] private Light _lampLight;

    [SerializeField] private bool firstPerson;
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
        // Movement components
        GetComponentInChildren<CharacterController>();

        // Network component
        _photonView = GetComponent<PhotonView>();

        // Camera component
        _postProcLayers = cameraHolder.GetComponentsInChildren<PostProcessLayer>();
        _audioListener = cameraHolder.GetComponentInChildren<AudioListener>();

        if (null == _camPlayer) throw new Exception("There is no camera attached to the Camera Holder !");

        // Sub scripts
        _playerMovement = GetComponent<PlayerMovement>();
        _playerLook = GetComponent<PlayerLook>();
    }

    internal void Start()
    {
        if (_photonView.IsMine)
        {
            // Moves the player render backwards so that it doesn't clip with the camera
            MoveRender(backShift, VillagerRender);

            // Turns the cam for the player render on
            _camPlayer.gameObject.SetActive(true);
            
            // Starts the Ai
            if (enableAi) StartCoroutine(AiCreator());
        }
        else
        {
            foreach (var layer in _postProcLayers) Destroy(layer);
            Destroy(_camPlayer);
            Destroy(_audioListener);
            playerInput.enabled = false;
            enableAi = false;
        }
    
        // Starts to grab players
        StartCoroutine(FindPlayers());
        
        // Starts the light management
        StartCoroutine(LightManager());
    }

    private IEnumerator FindPlayers()
    {
        yield return new WaitForSeconds(5);
        
        while (true)
        {
            yield return new WaitForSeconds(2);
            
            try
            {
                if (_role == null) _role = RoomManager.Instance.players.Find(r => r.GetComponent<PhotonView>().IsMine);

                var t = RoomManager.Instance.players;
                if (t.Count != playerPositions.Count)
                {
                    playerPositions = new List<Transform>();
                    foreach (Role role in t)
                        if (role.userId != _role.userId)
                            playerPositions.Add(role.gameObject.transform);
                }
            }
            catch
            {
                Debug.LogWarning("No RoomManager found ! (PlayerController)");
                yield break;
            }
        }
    }

    public void MoveRender(float shift, GameObject render, float smoothTime = 1)
    {
        if (!firstPerson) return;
        
        smoothTime = Mathf.Clamp01(smoothTime);
        
        Vector3 transformLocalPosition = render.transform.localPosition;
        transformLocalPosition.z = Mathf.Lerp(transformLocalPosition.z, shift, smoothTime);
        render.transform.localPosition = transformLocalPosition;
    }

    private IEnumerator LightManager()
    {
        _lampLight.intensity = 0;

        // Non-werewolves don't see lights
        RoomManager roomManager = RoomManager.Instance;

        if (roomManager == null) yield break;

        if (roomManager.localPlayer.GetType() == typeof(Werewolf) && _photonView.IsMine)
            while (true)
            {
                // It's day, turn off light
                _lampLight.intensity = 0;

                yield return new WaitUntil(() => VoteMenu.Instance.isNight);

                // It's night, turn on light
                _lampLight.intensity = 1;

                yield return new WaitUntil(() => !VoteMenu.Instance.isNight);
            }
    }

    private IEnumerator AiCreator()
    {
        yield return new WaitUntil(() => _role != null);

        if (RoomManager.Instance != null)
        {
            // Werewolves are not affected
            if (RoomManager.Instance.localPlayer is Werewolf) yield break;

            // Gets the village
            GameObject village = GameObject.FindWithTag("village");
            if (village != null) villageTransform = village.transform;
        }
        
        while (true)
        {
            if (CanAiSpawn())
            {
                // Can spawn the Ai
                AiInstance = Instantiate(AiPrefab,
                    transform.position + transform.TransformDirection(Vector3.back * 10 + Vector3.up * 2),
                    Quaternion.identity);

                // Ai spawn sound
                playerAudioSource.clip = aiSound;
                playerAudioSource.Play();
                
                AiController a = AiInstance.GetComponent<AiController>();
                a.targetRole = _role;

                hasAlreadySpawnedTonight = true;

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
            if (!VoteMenu.Instance.isNight) hasAlreadySpawnedTonight = false;
        }
        catch
        {
            hasAlreadySpawnedTonight = false;
        }

        if (hasAlreadySpawnedTonight)
            // Debug.Log("SPAWNCHECK (0/5): Already spawn tonight");
            return false;

        // Already spawned check
        if (IaAlreadySpawned)
            // Debug.Log("SPAWNCHECK (1/5): Ai already exists");
            return false;

        try
        {
            // Alive check
            if (_role != null && !_role.isAlive)
                // Debug.Log("SPAWNCHECK (2/5): is dead");
                return false;

            // Day check
            if (!VoteMenu.Instance.isNight)
                // Debug.Log("SPAWNCHECK (3/5): it's not night", VoteMenu.Instance.gameObject);
                return false;

            // Village check
            var villageTooClose = (villageTransform.position - transform.position).sqrMagnitude <
                                  minVillageDist * minVillageDist;
            if (villageTooClose)
                // Debug.Log("SPAWNCHECK (4/5): village is too close");
                return false;

            // Player check
            var everyPlayerFarEnough = true;
            for (var i = 0; i < playerPositions.Count && everyPlayerFarEnough; i++)
                everyPlayerFarEnough &= (playerPositions[i].position - transform.position).sqrMagnitude >
                                        minPlayerDist * minPlayerDist;

            if (!everyPlayerFarEnough)
                // Debug.Log("SPAWNCHECK (5/5): a player is too close");
                return false;
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

            _playerMovement.UpdateHitbox();
        }
        else
        {
            _playerLook.HeadRotate();
        }
    }

    #endregion
}