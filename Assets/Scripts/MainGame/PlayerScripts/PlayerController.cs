using System;
using System.Collections;
using System.Linq;
using MainGame;
using MainGame.PlayerScripts;
using MainGame.PlayerScripts.Roles;
using MainGame.PlayerScripts.Roles.Countdown;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerLook))]
[RequireComponent(typeof(PlayerAnimation))]
public class PlayerController : MonoBehaviour
{
    #region Attributes

    // Layer corresponding to the player
    public const int CharacterLayerValue = 7;
    
    // All components
        public PlayerController playerController;
        public PlayerMovement playerMovement;
        public PlayerLook playerLook;
        public PlayerAnimation playerAnimation;
        public SpectatorMode spectatorMode;
        public FootstepEffect footstepEffect;
        public PhotonView photonView;
        public PhotonAnimatorView photonAnimatorView;
        public CharacterController characterController;
        public Role role;
        public PlayerInput playerInput;
        public AudioListener audioListener;
        public Camera camPlayer;
        public PostProcessVolume postProcessVolume;
        public SkinnedMeshRenderer villagerSkinnedMeshRenderer;
        public GameObject camHolder, villagerRender, werewolfRender;
        public AudioSource playerAudioSource;
        public Light playerLight;
        public RotationConstraint headRotationConstraint;
        public Countdown powerCooldown, powerTimer;
        public GameObject dissimulateParticles;
    
    // Serialized fields
    [SerializeField] private AudioClip aiSound;
    [SerializeField] private GameObject aiPrefab;
    private GameObject _aiInstance;
    private Transform _villageTransform;
    
    private bool IaAlreadySpawned => null != _aiInstance;
    private bool _hasAlreadySpawnedTonight;
    [SerializeField] private bool enableAi = true, firstPerson;
    public const float BackShift = -0.3f;
    private const float MinVillageDist = 120, MinPlayerDist = 60;

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
        // Get All Components
        playerController = this;
        playerMovement = GetComponent<PlayerMovement>();
        playerLook = GetComponent<PlayerLook>();
        playerAnimation = GetComponent<PlayerAnimation>();
        spectatorMode = GetComponent<SpectatorMode>();
        footstepEffect = GetComponent<FootstepEffect>();
        photonView = GetComponent<PhotonView>();
        photonAnimatorView = GetComponent<PhotonAnimatorView>();
        role = GetComponent<Role>();
        playerInput = GetComponent<PlayerInput>();
        audioListener = GetComponentInChildren<AudioListener>();
        playerAudioSource = GetComponent<AudioSource>();
        camPlayer = GetComponentInChildren<Camera>();
        postProcessVolume = GetComponentInChildren<PostProcessVolume>();
        villagerSkinnedMeshRenderer = villagerRender.GetComponentInChildren<SkinnedMeshRenderer>();
        powerCooldown = GetComponents<Countdown>()[0];
        powerTimer = GetComponents<Countdown>()[1];

        if (null == camPlayer) throw new Exception("There is no camera attached to the Camera Holder !");
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            // Moves the player render backwards so that it doesn't clip with the camera
            MoveRender(BackShift, villagerRender);

            // Turns the cam for the player render on
            camPlayer.gameObject.SetActive(true);

            // Starts the Ai
            if (enableAi) StartCoroutine(AiCreator());
        }
        else
        {
            Destroy(postProcessVolume);
            Destroy(camPlayer);
            Destroy(audioListener);
            playerInput.enabled = false;
            enableAi = false;
        }

        // Starts the light management
        StartCoroutine(LightManager());
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
        playerLight.intensity = 0;

        // Non-werewolves don't see lights
        RoomManager roomManager = RoomManager.Instance;

        if (!roomManager) yield break;

        if (roomManager.localPlayer is Werewolf && photonView.IsMine)
            while (true)
            {
                // It's day, turn off light
                playerLight.intensity = 0;

                yield return new WaitUntil(() => VoteMenu.Instance.IsNight);

                // It's night, turn on light
                // ReSharper disable once Unity.InefficientPropertyAccess
                playerLight.intensity = 1;

                yield return new WaitUntil(() => !VoteMenu.Instance.IsNight);
            }
    }

    private IEnumerator AiCreator()
    {
        yield return new WaitUntil(() => role);

        if (RoomManager.Instance)
        {
            // Werewolves are not affected
            if (RoomManager.Instance.localPlayer is Werewolf) yield break;

            // Gets the village
            GameObject village = GameObject.FindWithTag("village");
            if (village) _villageTransform = village.transform;
        }

        while (true)
        {
            if (CanAiSpawn())
            {
                // Can spawn the Ai
                _aiInstance = Instantiate(aiPrefab,
                    transform.position + transform.TransformDirection(Vector3.back * 10 + Vector3.up * 2),
                    Quaternion.identity);

                // Ai spawn sound
                playerAudioSource.clip = aiSound;
                playerAudioSource.Play();

                AiController a = _aiInstance.GetComponent<AiController>();
                a.targetRole = role;

                _hasAlreadySpawnedTonight = true;

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
            if (!VoteMenu.Instance.IsNight) _hasAlreadySpawnedTonight = false;
        }
        catch
        {
            _hasAlreadySpawnedTonight = false;
        }

        if (_hasAlreadySpawnedTonight)
            // Debug.Log("SPAWNCHECK (0/5): Already spawn tonight");
            return false;

        // Already spawned check
        if (IaAlreadySpawned)
            // Debug.Log("SPAWNCHECK (1/5): Ai already exists");
            return false;

        try
        {
            // Alive check
            if (role && !role.isAlive)
                // Debug.Log("SPAWNCHECK (2/5): is dead");
                return false;

            // Day check
            if (!VoteMenu.Instance.IsNight)
                // Debug.Log("SPAWNCHECK (3/5): it's not night", VoteMenu.Instance.gameObject);
                return false;

            // Village check
            bool villageTooClose = (_villageTransform.position - transform.position).sqrMagnitude <
                                   MinVillageDist * MinVillageDist;
            if (villageTooClose)
                // Debug.Log("SPAWNCHECK (4/5): village is too close");
                return false;

            // Player check
            bool everyPlayerFarEnough = RoomManager.Instance.players.All(role =>
                !((role.transform.position - transform.position).sqrMagnitude > MinPlayerDist * MinPlayerDist));

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
        // Updates the grounded boolean state
        playerMovement.UpdateGrounded();

        if (photonView.IsMine)
        {
            playerLook.Look();

            // Moves the player
            playerMovement.Move(Time.deltaTime);

            // Updates the appearance based on the MovementType
            playerAnimation.UpdateAnimationsBasic();

            playerMovement.UpdateHitbox();

            // HeadBob
            playerLook.HeadBob();

            // FOV Change according to movement
            playerLook.FOVChanger();

            // Focus DOF
            playerLook.DofChanger();
        }
    }

    private void OnAnimatorIK(int _)
    {
        if (!photonView.IsMine)
        {
            playerLook.HeadRotate();
        }
    }

    #endregion

    public void SetPcRole(Role playerRole) => role = playerRole;
}