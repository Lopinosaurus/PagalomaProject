using System.Collections;
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
        protected bool arePowerAndCooldownValid => powerCooldown.isZero && powerTimer.isNotZero;

        
        protected TMP_Text actionText;
        public TMP_Text deathText;
        public Role vote;

        public Color color;
        private PostProcessVolume _postProcessVolume;

        // Controls
        [SerializeField] private GameObject _cameraHolder;
        public PlayerInput _playerInput;
        protected PlayerMovement _playerMovement;
        private PlayerController _playerController;
        protected PlayerAnimation _playerAnimation;
        private CharacterController _characterController;

        // Die variables
        private const float maxDeathCamDistance = 5;
        private RotationConstraint _rotationConstraint;

        // Network component
        protected PhotonView _photonView; // Use protected to be able to access it in subclasses

        #endregion

        #region Unity Methods

        private void Awake()
        {
            isActive = false;
            hasVoted = false;
            
            _playerInput = GetComponent<PlayerInput>();
            _playerController = GetComponent<PlayerController>();
            _playerAnimation = GetComponent<PlayerAnimation>();
            _playerMovement = GetComponent<PlayerMovement>();
            _characterController = GetComponent<CharacterController>();
            _cameraHolder.GetComponentInChildren<Camera>();
            _photonView = GetComponent<PhotonView>();
            
            if (RoomManager.Instance != null)
            {
                actionText = RoomManager.Instance.actionText;
                deathText = RoomManager.Instance.deathText;
            }

            if (actionText != null) actionText.text = "";
            if (deathText != null) deathText.enabled = false;

            _rotationConstraint = GetComponentInChildren<RotationConstraint>();
            _postProcessVolume = GetComponentInChildren<PostProcessVolume>();

            _postProcessVolume.profile.GetSetting<Vignette>().color.value = color;

            powerCooldown = gameObject.AddComponent<Countdown.Countdown>();
            powerTimer = gameObject.AddComponent<Countdown.Countdown>();
        }
        
        public void Activate()
        {
            isActive = true;
            
            if (_photonView.IsMine)
            {
                _playerInput.actions["Kill"].started += ctx => kill = ctx.ReadValueAsButton();
                _playerInput.actions["Kill"].canceled += ctx => kill = ctx.ReadValueAsButton();
                _playerInput.actions["Click"].performed += _ => PlayerInteraction.Instance.Click();
            }
        }

        private void LateUpdate()
        {
            if (kill) UseAbility();

            if (VoteMenu.Instance && !VoteMenu.Instance.isNight && hasShield) hasShield = false;
        }

        #endregion

        #region Gameplay methods

        public void SetPlayerColor(Color _color)
        {
            color = _color;
            Transform find = gameObject.transform.Find("VillagerRender");
            bool wasActive = find.gameObject.activeSelf;

            find.gameObject.SetActive(true);

            find.GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[1].color = _color;

            find.gameObject.SetActive(wasActive);
        }

        public virtual void UseAbility()
        {
            Debug.Log("E pressed but you are have not ability because you are a Villager. (Villager < all UwU)");
        }
        
        protected bool CanUseAbilityGeneric()
        {
            if (!VoteMenu.Instance.isNight) return false;
            if (!arePowerAndCooldownValid) return false;
            if (!isAlive) return false;

            return true;
        }

        public virtual void UpdateActionText()
        {
            Debug.Log($"In {nameof(UpdateActionText)} but you have no action text");
        }

        public void Die()
        {
            // Show death label & disable inputs
            if (_photonView.IsMine)
            {
                deathText.enabled = true;
                _playerInput.actions["Die"].Disable();
                _playerInput.actions["Kill"].Disable();
            }

            _playerAnimation.EnableDeathAppearance();

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
                Vector3 startingPos = _cameraHolder.transform.position;
                Vector3 endingPos = new Vector3
                {
                    x = startingPos.x,
                    y = startingPos.y + maxDeathCamDistance,
                    z = startingPos.z
                };

                // Final camera position
                if (Physics.Raycast(startingPos, Vector3.up, out RaycastHit hitInfo, maxDeathCamDistance))
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

            if (_photonView.IsMine) GetComponent<SpectatorMode>().isSpectatorModeEnabled = true;
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