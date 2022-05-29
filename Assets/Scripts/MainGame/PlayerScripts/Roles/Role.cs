using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

namespace MainGame.PlayerScripts.Roles
{
    public class Role : MonoBehaviour
    {
        #region Attributes

        public bool isActive;

        // Gameplay attributes
        public string roleName;
        public bool isAlive = true;
        public bool hasCooldown;
        public bool hasShield; // Shield given by the Priest
        public string username;
        public string userId;
        public Color color;
        public Role vote;
        public bool hasVoted; // Has submitted vote this day
        protected TMP_Text actionText;
        private TMP_Text deathText;
        
        [SerializeField] private bool kill;

        // Controls
        [SerializeField] private GameObject _cameraHolder;
        public PlayerInput _playerInput;
        protected PlayerMovement _playerMovement;
        private PlayerController _playerController;
        protected PlayerAnimation _playerAnimation;
        private CharacterController _characterController;
        private Camera cam;

        // Die variables
        private const float maxDeathCamDistance = 5.0f;
        private RotationConstraint _rotationConstraint;

        // Network component
        protected PhotonView _photonView; // Use protected to be able to access it in subclasses

        #endregion

        #region Unity Methods

        private void Awake()
        {
            isActive = false;
            _playerInput = GetComponent<PlayerInput>();
            _playerController = GetComponent<PlayerController>();
            _playerAnimation = GetComponent<PlayerAnimation>();
            _playerMovement = GetComponent<PlayerMovement>();
            _characterController = GetComponent<CharacterController>();
            cam = _cameraHolder.GetComponentInChildren<Camera>();
            _photonView = GetComponent<PhotonView>();
            if (RoomManager.Instance != null)
            {
                actionText = RoomManager.Instance.actionText;
                deathText = RoomManager.Instance.deathText;
            }
            if (actionText != null) actionText.text = "";
            if (deathText != null) deathText.enabled = false;
            hasVoted = false;

            _rotationConstraint = GetComponentInChildren<RotationConstraint>();
        }

        public void Activate()
        {
            isActive = true;
            if (_photonView.IsMine)
            {
                _playerInput.actions["Kill"].started += ctx => kill = ctx.ReadValueAsButton();
                _playerInput.actions["Kill"].canceled += ctx => kill = ctx.ReadValueAsButton();
                _playerInput.actions["Click"].performed += ctx => PlayerInteraction.Instance.Click();
            }
        }

        private void LateUpdate()
        {
            if (kill) UseAbility();
        }

        #endregion

        #region Gameplay methods

        public void SetPlayerColor(Color _color)
        {
            color = _color;
            gameObject.transform.Find("VillagerRender").GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[1].color = _color;
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
                deathText.enabled = false;
                _playerInput.actions["Die"].Disable();
                _playerInput.actions["Kill"].Disable();
            }
            
            //
            _rotationConstraint.enabled = false;

            // Disable components & gameplay variables
            _characterController.enabled = false;
            _playerController.enabled = false;
            isAlive = false;

            RoomManager.Instance.ClearTargets();
            VoteMenu.Instance.UpdateVoteItems();

            // Initial camera position
            if ((bool)_cameraHolder)
            {
                Vector3 startingPos = _cameraHolder.transform.position;
                Vector3 endingPos = new Vector3
                {
                    x = startingPos.x,
                    y = startingPos.y + maxDeathCamDistance,
                    z = startingPos.z
                };

                _playerAnimation.EnableDeathAppearance();

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
                // Debug.Log("endingRot is:" + endingRot);
                // Start camera animation
                StartCoroutine(MoveCamHolder(endingPos, endingRot));
            }
        }

        private IEnumerator MoveCamHolder(Vector3 endingPos, Quaternion endingRot)
        {
            while (_cameraHolder.transform.position != endingPos)
            {
                Vector3 position = _cameraHolder.transform.position;
                Quaternion rotation = _cameraHolder.transform.localRotation;

                position = Vector3.Slerp(position, endingPos, 0.02f);
                rotation = Quaternion.Slerp(rotation, endingRot, 0.05f);

                _cameraHolder.transform.position = position;
                _cameraHolder.transform.localRotation = rotation;
                yield return null;
            }
        }

        #endregion
    }
}