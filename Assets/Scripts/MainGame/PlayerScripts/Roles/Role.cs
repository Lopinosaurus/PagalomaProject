using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

namespace MainGame.PlayerScripts.Roles
{
    public abstract class Role : MonoBehaviour
    {
        #region Attributes

        // Gameplay attributes
        public string roleName;
        public string username;
        public string userId;
        
        [SerializeField] private bool kill;
        public bool isAlive = true;
        public bool hasShield; // Shield given by the Priest
        public bool hasVoted; // Has submitted vote this day

        // Power cooldown (timer you have to wait before you can activate your power)
        protected Countdown.Countdown PowerCooldown;
        // Power timer (timer during which you can make use of your power)
        protected Countdown.Countdown PowerTimer;
        protected bool ArePowerAndCooldownValid => PowerCooldown.IsZero && PowerTimer.IsNotZero;

        // Internal components
        protected SkinnedMeshRenderer VillagerSkinnedMeshRenderer;
        
        protected TMP_Text ActionText;
        public TMP_Text deathText;
        public Role vote;

        public Color color;
        private PostProcessVolume _postProcessVolume;

        // Controls
        private GameObject _cameraHolder;
        protected PlayerInput PlayerInput;
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
            PhotonView = GetComponent<PhotonView>();
            PlayerInput = GetComponent<PlayerInput>();
            
            if (PhotonView.IsMine)
            {
                PlayerInput.actions["Kill"].started += ctx => kill = ctx.ReadValueAsButton();
                PlayerInput.actions["Kill"].canceled += ctx => kill = ctx.ReadValueAsButton();
                PlayerInput.actions["Click"].performed += _ => PlayerInteraction.Instance.Click();
            }
            
            hasVoted = false;
            
            PlayerInput = GetComponent<PlayerInput>();
            _playerController = GetComponent<PlayerController>();
            PlayerAnimation = GetComponent<PlayerAnimation>();
            PlayerMovement = GetComponent<PlayerMovement>();
            
            _cameraHolder = _playerController.cameraHolder;
            _characterController = GetComponent<CharacterController>();
            VillagerSkinnedMeshRenderer = gameObject.transform.Find("VillagerRender").GetComponentInChildren<SkinnedMeshRenderer>();
            
            if (RoomManager.Instance != null)
            {
                ActionText = RoomManager.Instance.actionText;
                deathText = RoomManager.Instance.deathText;
            }

            if (ActionText) ActionText.text = "";
            if (deathText) deathText.enabled = false;

            _rotationConstraint = GetComponentInChildren<RotationConstraint>();
            _postProcessVolume = GetComponentInChildren<PostProcessVolume>();

            _postProcessVolume.profile.GetSetting<Vignette>().color.value = color;

            // Countdown instances
            Countdown.Countdown[] countdownInstances = GetComponents<Countdown.Countdown>();
            PowerCooldown = countdownInstances[0];
            PowerTimer = countdownInstances[1];
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
            VillagerSkinnedMeshRenderer.materials[1].color = color;
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

        public void Die()
        {
            // Show death label & disable inputs
            if (PhotonView.IsMine)
            {
                deathText.enabled = true;
                PlayerInput.actions["Die"].Disable();
                PlayerInput.actions["Kill"].Disable();
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
            if (_cameraHolder)
            {
                // Detach cameraHolder from body
                _cameraHolder.transform.parent = null;
                
                Vector3 startingPos = _cameraHolder.transform.position;
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
                Quaternion endingRot = _cameraHolder.transform.localRotation;
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
            while (_cameraHolder.transform.position != endingPos && timer > 0)
            {
                Vector3 position = _cameraHolder.transform.position;
                Quaternion rotation = _cameraHolder.transform.localRotation;

                position = Vector3.Slerp(position, endingPos, Time.deltaTime);
                rotation = Quaternion.Slerp(rotation, endingRot, Time.deltaTime);

                _cameraHolder.transform.position = position;
                _cameraHolder.transform.localRotation = rotation;

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
                PowerTimer.SetInfinite();
                PowerCooldown.Reset();
            }
            else
            {
                PowerTimer.Reset();
            }

            PowerCooldown.Resume();
            PowerTimer.Resume();
        }

        public virtual void UpdateTarget(Collider other, bool b) {}
    }
}