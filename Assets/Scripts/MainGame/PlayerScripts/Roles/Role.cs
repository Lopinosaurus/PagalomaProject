using System.Collections;
using System.Collections.Generic;
using MainGame.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static MainGame.Helpers.QuestManager.Quest;

namespace MainGame.PlayerScripts.Roles
{
    public abstract class Role : MonoBehaviour
    {
        #region Attributes

        // For UI Action text
        public enum AtMessage
        {
            PowerReadyToUse,
            PowerReadyToEnable,
            PowerOnCooldown,
            Clear
        };
        protected Dictionary<AtMessage, string> AtMessageDict;
        private string GetAtText(AtMessage atMessage) => AtMessageDict[atMessage];
        protected virtual AtMessage GetAtMessage()
        {
            if (ArePowerAndCooldownValid) return AtMessage.PowerReadyToUse;
            return AtMessage.PowerOnCooldown;
        }

        // Quests
        private QuestManager.Quest _currentQuest;
        
        public QuestManager.Quest CurrentQuest
        {
            get => _currentQuest;
            set
            {
                LastQuest = _currentQuest;
                _currentQuest = value; 
            }
        }

        public QuestManager.Quest LastQuest { get; private set; } = None;

        // Gameplay attributes
        public string roleName;
        public string username;
        public string userId;
        
        [SerializeField] private bool kill;
        public bool isAlive = true;
        public bool isShielded; // Shield given by the Priest
        public bool hasVoted; // Has submitted vote this day

        protected bool ArePowerAndCooldownValid => PlayerController.powerCooldown.IsZero && PlayerController.powerTimer.IsNotZero;

        protected TMP_Text ActionText;
        public TMP_Text deathText;
        public Role vote;

        public Color color;
        private PostProcessVolume _postProcessVolume;

        private PlayerController _playerController;
        protected PlayerController PlayerController
        {
            get
            {
                if (_playerController) return _playerController;
                _playerController = GetComponent<PlayerController>();
                return _playerController;
            }
        }

        // Die variables
        private const float MaxDeathCamDistance = 5;

        #endregion

        #region Unity Methods

        protected void Awake()
        {
            if (PlayerController.photonView.IsMine)
            {
                PlayerController.playerInput.actions["Kill"].performed += ctx =>
                {
                    kill = ctx.ReadValueAsButton();
                    UseAbility();
                };
                PlayerController.playerInput.actions["Kill"].canceled += ctx => kill = ctx.ReadValueAsButton();
                PlayerController.playerInput.actions["Click"].performed += _ => PlayerInteraction.Instance.Click();
            }
            
            hasVoted = false;

            ActionText = RoomManager.Instance.actionText;
            deathText = RoomManager.Instance.deathText;

            ActionText.text = "";
            deathText.enabled = false;

            PlayerController.postProcessVolume.profile.GetSetting<Vignette>().color.value = color;
        }

        #endregion

        #region Gameplay methods

        public void SetPlayerColor(Color color)
        {
            this.color = color;
            PlayerController.villagerSkinnedMeshRenderer.materials[1].color = color;
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

        public virtual void UpdateActionText(AtMessage message)
        {
            if (!PlayerController.photonView.IsMine) return;
            ActionText.text = GetAtText(message);
        }

        public virtual void Die()
        {
            // Show death label & disable inputs
            if (PlayerController.photonView.IsMine)
            {
                deathText.enabled = true;
                PlayerController.playerInput.actions["Die"].Disable();
                PlayerController.playerInput.actions["Kill"].Disable();
            }

            PlayerController.playerAnimation.EnableDeathAppearance();

            // Prevents the head for rotating in the ground when dying
            PlayerController.headRotationConstraint.enabled = false;

            // Disable components & gameplay variables
            PlayerController.characterController.enabled = false;
            PlayerController.enabled = false;
            isAlive = false;

            RoomManager.Instance.ClearTargets();
            VoteMenu.Instance.UpdateVoteItems();

            // Initial camera position
            if (PlayerController.camHolder)
            {
                // Detach cameraHolder from body
                PlayerController.camHolder.transform.parent = null;
                
                Vector3 startingPos = PlayerController.camHolder.transform.position;
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
                Quaternion endingRot = PlayerController.camHolder.transform.localRotation;
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
            while (PlayerController.camHolder.transform.position != endingPos && timer > 0)
            {
                Vector3 position = PlayerController.camHolder.transform.position;
                Quaternion rotation = PlayerController.camHolder.transform.localRotation;

                position = Vector3.Slerp(position, endingPos, Time.deltaTime);
                rotation = Quaternion.Slerp(rotation, endingRot, Time.deltaTime);

                PlayerController.camHolder.transform.position = position;
                PlayerController.camHolder.transform.localRotation = rotation;

                timer -= Time.deltaTime;

                yield return null;
            }

            if (PlayerController.photonView.IsMine) PlayerController.spectatorMode.isSpectatorModeEnabled = true;
        }

        #endregion

        public void SetCountdowns(bool isNight)
        {
            if (isNight)
            {
                PlayerController.powerTimer.SetInfinite();
                PlayerController.powerCooldown.Reset();
            }
            else
            {
                PlayerController.powerTimer.Reset();
            }

            PlayerController.powerCooldown.Resume();
            PlayerController.powerTimer.Resume();
        }

        public virtual void UpdateTarget(Collider other, bool b) {}

        public void Reset() => vote = null;
    }
}