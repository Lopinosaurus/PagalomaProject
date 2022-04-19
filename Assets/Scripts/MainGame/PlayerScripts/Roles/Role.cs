using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = System.Object;

namespace MainGame.PlayerScripts.Roles
{
    public class Role : MonoBehaviour
    {
        #region Attributes

        public bool isActive;
        // Gameplay attributes
        public string roleName;
        public bool isAlive = true;
        public bool hasCooldown = false;
        public string username;
        public string userId;
        public Color color;
        public Role vote;
        [SerializeField] protected TMP_Text actionText;
        [SerializeField] protected TMP_Text deathText;

        [SerializeField] private bool selfKill = false;
        [SerializeField] private bool kill = false;

        // Controls
        [SerializeField] private GameObject _cameraHolder;
        public PlayerInput _playerInput;
        private PlayerMovement _playerMovement;
        private PlayerController _playerController;
        private PlayerLook _playerLook;
        private PlayerAnimation _playerAnimation;
        private CharacterController _characterController;
        private Camera cam;
        
        // Die variables
        private const float maxDeathCamDistance = 5.0f;
        
        // Network component
        protected PhotonView _photonView; // Use protected to be able to access it in subclasses
        
        #endregion

       #region Unity Methods

       private void Awake()
       {
           isActive = false;
           _playerInput = GetComponent<PlayerInput>();
           _playerController = GetComponent<PlayerController>();
           _playerLook = GetComponent<PlayerLook>();
           _playerAnimation = GetComponent<PlayerAnimation>();
           _playerMovement = GetComponent<PlayerMovement>();
           _characterController = GetComponent<CharacterController>();
           cam = _cameraHolder.GetComponentInChildren<Camera>();
           _photonView = GetComponent<PhotonView>();
           actionText = RoomManager.Instance.actionText;
           deathText = RoomManager.Instance.deathText;
           actionText.text = "";
           deathText.enabled = false;
       }

       public void Activate()
       {
           isActive = true;
           if (_photonView.IsMine)
           {
               _playerInput.actions["Die"].started += ctx => selfKill = ctx.ReadValueAsButton();
               _playerInput.actions["Kill"].started += ctx => kill = ctx.ReadValueAsButton();
               _playerInput.actions["Kill"].canceled  += ctx => kill = ctx.ReadValueAsButton();
               _playerInput.actions["Click"].performed += ctx => PlayerInteraction.Instance.Click();
           }
       }

       private void LateUpdate()
       {
           if (selfKill && isAlive) Die();
           if (kill) UseAbility();
       }

       #endregion
       
       #region Gameplay methods

       public void SetPlayerColor(Color color)
       {
           this.color = color;
           // TODO Update appearance
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
           // Disable components & gameplay variables
           _characterController.detectCollisions = false;
           _playerController.enabled = false;
           isAlive = false;

           VoteMenu.Instance.UpdateVoteItems();
           
           // Initial camera position
           if (_cameraHolder != null)
           {
               Vector3 startingPos = _cameraHolder.transform.position;
               Quaternion startingRot = _cameraHolder.transform.rotation;
               Vector3 endingPos = new Vector3
               {
                   x = startingPos.x,
                   y = startingPos.y + maxDeathCamDistance,
                   z = startingPos.z
               };
               // Debug.Log("startingRot is:" + startingRot);
               
               // Final camera position
               if (Physics.Raycast(startingPos, Vector3.up, out RaycastHit hitInfo, maxDeathCamDistance))
               {
                   endingPos.y = hitInfo.point.y - 0.2f;
               }
               
               // Final camera rotation
               Quaternion endingRot = Quaternion.identity;
               endingRot.eulerAngles = new Vector3
               {
                   x = 90,
                   y = endingRot.eulerAngles.y,
                   z = 180,
               };
               // Debug.Log("endingRot is:" + endingRot);
               // Start camera animation
               StartCoroutine(MoveCamHolder(endingPos, endingRot));
            }
           _playerAnimation.EnableDeathAppearance();
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
