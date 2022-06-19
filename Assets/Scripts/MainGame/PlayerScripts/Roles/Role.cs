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

        // Power cooldown
        protected bool hasCooldown => cooldown > 0;
        private float currentlyUsedCooldownMult = 1;
        private float cooldown;
        
        // Power timer (how long does the power stay active)
        protected bool isPowerTimerOngoing => powerTimer > 0;
        private float currentlyUsedPowerTimerMult = 1;
        private float powerTimer;
        
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

            StartCoroutine(CooldownAndPowerTimerManager());
        }

        private IEnumerator CooldownAndPowerTimerManager()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            
            while (true)
            {
                if (cooldown > 0) cooldown -= currentlyUsedCooldownMult * Time.fixedDeltaTime;
                if (powerTimer > 0) powerTimer -= currentlyUsedPowerTimerMult * Time.fixedDeltaTime;

                yield return waitForFixedUpdate;
            }
        }

        // Power cooldown
        private void PauseCooldown() => SetCooldownMultiplier(0);
        private void ResumeCooldown() => SetCooldownMultiplier(1);
        protected void SetCooldown(float value) => cooldown = value < 0 ? 0 : value;
        public void ResetCooldown() => SetCooldown(0);
        public void SetCooldownInfinite() => SetCooldown(float.MaxValue);
        private void SetCooldownMultiplier(float newMultiplier) => currentlyUsedCooldownMult = newMultiplier < 0 ? 0 : newMultiplier;
        
        // Power timer (how long does it stay active ?)
        protected void PausePowerTimer() => SetPowerTimerMultiplier(0);

        protected void ResumePowerTimer(float delayInSeconds = 0)
        {
            if (0 == delayInSeconds) SetCooldownMultiplier(1);
            else StartCoroutine(ResumePowerTimerCoroutine(delayInSeconds));
        }

        private IEnumerator ResumePowerTimerCoroutine(float delayInSeconds)
        {
            yield return new WaitForSeconds(delayInSeconds);
            ResumePowerTimer();
        }
        
        protected void SetPowerTimer(float value) => powerTimer = value < 0 ? 0 : value;
        public void ResetPowerTimer() => SetPowerTimer(0);
        public void SetPowerTimerInfinite() => SetPowerTimer(float.MaxValue);
        private void SetPowerTimerMultiplier(float newMultiplier) => currentlyUsedPowerTimerMult = newMultiplier < 0 ? 0 : newMultiplier;

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

            if (VoteMenu.Instance != null && !VoteMenu.Instance.isNight && hasShield) hasShield = false;
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

        public virtual void UpdateActionText()
        {
            Debug.Log("In UpdateActionText() but you have no action text");
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
            if (null != _cameraHolder)
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
                    endingPos.y = hitInfo.point.y - 0.2f;

                // Final camera rotation
                Quaternion endingRot = Quaternion.identity;
                endingRot.eulerAngles = new Vector3
                {
                    x = 90,
                    y = endingRot.eulerAngles.y,
                    z = 180
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

    }
}