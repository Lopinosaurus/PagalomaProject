using System.Collections;
using System.Runtime.Remoting.Contexts;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

namespace MainGame.PlayerScripts.Roles
{
    public class Role : MonoBehaviour
    {
        #region Attributes

        public bool isActive;

        // Gameplay attributes
        public string roleName;
        public string username;
        public string userId;
        
        [SerializeField] private bool kill;
        public bool isAlive = true;
        public bool hasShield; // Shield given by the Priest
        public bool hasVoted; // Has submitted vote this day

        // Power cooldown (timer you have to wait before you can activate your power)
        public Countdown.Countdown powerCooldown;
        // Power timer (timer during which you can make use of your power)
        public Countdown.Countdown powerTimer;
        protected bool ArePowerAndCooldownValid => powerCooldown.IsZero && powerTimer.IsNotZero;

        
        protected TMP_Text ActionText;
        public TMP_Text deathText;
        public Role vote;

        public Color color;
        private PostProcessVolume _postProcessVolume;

        // Controls
        [SerializeField] private GameObject cameraHolder;
        public PlayerInput playerInput;
        protected PlayerMovement PlayerMovement;
        private PlayerController _playerController;
        protected PlayerAnimation PlayerAnimation;
        private CharacterController _characterController;

        // Die variables
        private const float MaxDeathCamDistance = 5;
        private RotationConstraint _rotationConstraint;

        // Network component
        protected PhotonView PhotonView; // Use protected to be able to access it in subclasses

        #endregion

        #region Unity Methods

        private void Awake()
        {
            isActive = false;
            hasVoted = false;
            
            playerInput = GetComponent<PlayerInput>();
            _playerController = GetComponent<PlayerController>();
            PlayerAnimation = GetComponent<PlayerAnimation>();
            PlayerMovement = GetComponent<PlayerMovement>();
            _characterController = GetComponent<CharacterController>();
            cameraHolder.GetComponentInChildren<Camera>();
            PhotonView = GetComponent<PhotonView>();
            
            if (RoomManager.Instance != null)
            {
                ActionText = RoomManager.Instance.actionText;
                deathText = RoomManager.Instance.deathText;
            }

            if (ActionText != null) ActionText.text = "";
            if (deathText != null) deathText.enabled = false;

            _rotationConstraint = GetComponentInChildren<RotationConstraint>();
            _postProcessVolume = GetComponentInChildren<PostProcessVolume>();

            _postProcessVolume.profile.GetSetting<Vignette>().color.value = color;

            // Countdown instances
            Countdown.Countdown[] countdownInstances = GetComponents<Countdown.Countdown>();
            powerCooldown = countdownInstances[0];
            powerTimer = countdownInstances[1];
        }
        
        public void Activate()
        {
            isActive = true;
            
            if (PhotonView.IsMine)
            {
                playerInput.actions["Kill"].started += ctx => kill = ctx.ReadValueAsButton();
                playerInput.actions["Kill"].canceled += ctx => kill = ctx.ReadValueAsButton();
                playerInput.actions["Click"].performed += _ => PlayerInteraction.Instance.Click();
            }
        }

        private void LateUpdate()
        {
            if (kill) UseAbility();

            if (VoteMenu.Instance && !VoteMenu.Instance.IsNight && hasShield) hasShield = false;
        }

        #endregion

        #region Gameplay methods

        public void SetPlayerColor(Color color)
        {
            this.color = color;
            Transform find = gameObject.transform.Find("VillagerRender");
            bool wasActive = find.gameObject.activeSelf;

            find.gameObject.SetActive(true);

            find.GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[1].color = color;

            find.gameObject.SetActive(wasActive);
        }

        public virtual void UseAbility()
        {
            Debug.Log("E pressed but you are have not ability because you are a Villager. (Villager < all UwU)");
        }
        
        protected bool CanUseAbilityGeneric()
        {
            if (!VoteMenu.Instance.IsNight) return false;
            if (!ArePowerAndCooldownValid) return false;
            if (!isAlive) return false;

            return true;
        }

        public virtual void UpdateActionText() => Debug.Log($"In {nameof(UpdateActionText)} but you have no action text");

        [ContextMenuItem("commands", nameof(Die))]
        private ContextMenu da;
        public void Die()
        {
            // Show death label & disable inputs
            if (PhotonView.IsMine)
            {
                deathText.enabled = true;
                playerInput.actions["Die"].Disable();
                playerInput.actions["Kill"].Disable();
            }

            PlayerAnimation.EnableDeathAppearance();

            // Prevents the head for rotating in the ground when dying
            _rotationConstraint.enabled = false;

            // Disable components & gameplay variables
            _characterController.enabled = false;
            _playerController.enabled = false;
            isAlive = false;

            RoomManager.Instance.ClearTargets();
            VoteMenu.Instance.UpdateVoteItems();

            // Initial camera position
            if (cameraHolder)
            {
                // Detach cameraHolder from body
                cameraHolder.transform.parent = null;
                
                Vector3 startingPos = cameraHolder.transform.position;
                Vector3 endingPos = new Vector3
                {
                    x = startingPos.x,
                    y = startingPos.y + MaxDeathCamDistance,
                    z = startingPos.z
                };

                // Final camera position
                if (Physics.Raycast(startingPos, Vector3.up, out RaycastHit hitInfo, MaxDeathCamDistance))
                    endingPos.y = hitInfo.point.y;

                // Final camera rotation
                Quaternion endingRot = cameraHolder.transform.localRotation;
                endingRot.eulerAngles = new Vector3
                {
                    x = 90,
                    y = endingRot.eulerAngles.y + 180
                };
                
                StartCoroutine(MoveCamHolder(endingPos, endingRot));
            }
        }

        private IEnumerator MoveCamHolder(Vector3 endingPos, Quaternion endingRot)
        {
            float timer = 10;
            while (cameraHolder.transform.position != endingPos && timer > 0)
            {
                Vector3 position = cameraHolder.transform.position;
                Quaternion rotation = cameraHolder.transform.localRotation;

                position = Vector3.Slerp(position, endingPos, Time.deltaTime);
                rotation = Quaternion.Slerp(rotation, endingRot, Time.deltaTime);

                cameraHolder.transform.position = position;
                cameraHolder.transform.localRotation = rotation;

                timer -= Time.deltaTime;

                yield return null;
            }

            if (PhotonView.IsMine) GetComponent<SpectatorMode>().isSpectatorModeEnabled = true;
        }

        #endregion

        public void SetCountdowns(bool isNight)
        {
            if (isNight)
            {
                powerTimer.SetInfinite();
                powerCooldown.Reset();
            }
            else
            {
                powerTimer.Reset();
            }

            powerCooldown.Resume();
            powerTimer.Resume();
        }
    }
}