using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = System.Object;

namespace MainGame.PlayerScripts.Roles
{
    public class Role : MonoBehaviour
    {
        #region Attributes
        
        // Gameplay attributes
        public bool isAlive;
        public string username;
        public string color;
        public Role vote;
        
        [SerializeField] private bool selfKill = false;

        // Controls
        [SerializeField] private GameObject _cameraHolder;
        private PlayerControls playerControls;
        private PlayerMovement _playerMovement;
        private PlayerController _playerController;
        private PlayerLook _playerLook;
        private CharacterController _characterController;
        private Camera cam;


        // Death camera coroutine
       private Vector3 velocity;

       public Role(string username, string color)
       {
           this.isAlive = true;
           this.username = username;
           this.color = color;
           this.vote = null;
       }

       #endregion

       #region Unity Methods

       private void Awake()
       {
           _playerController = GetComponent<PlayerController>();
           playerControls = _playerController.playerControls;
           _playerLook = GetComponent<PlayerLook>();
           _playerMovement = GetComponent<PlayerMovement>();
           _characterController = GetComponent<CharacterController>();
           cam = _cameraHolder.GetComponentInChildren<Camera>();
       }

       private void Start()
       {
           playerControls.Player.Die.started += ctx => selfKill = ctx.ReadValueAsButton();
       }

       private void LateUpdate()
       {
           if (selfKill)
           {
               Debug.Log("Die executed !");
               Die();
           }
       }

       #endregion
       
       #region Gameplay methods

       private IEnumerator coroutine;
       public void Die()
       {
           // Disable every control
           playerControls.Disable();

           // Disable collision
           _playerController.enabled = false;
           _characterController.detectCollisions = false;

           // Initial camera position
           Vector3 endingPos = _cameraHolder.transform.position;
           
           Debug.Log("endingPos is:" + endingPos);

           // Final camera position
           Vector3 rayOrigin = cam.WorldToViewportPoint(new Vector3(0.5f,0.5f,0));
           if (Physics.Raycast(rayOrigin, Vector3.up, out RaycastHit hitInfo, 5.1f))
           {
               endingPos = hitInfo.point;
           }
           
           StartCoroutine(MoveCameraOnDeath(endingPos));
       }
       
       // Moves the camera above the player
       private IEnumerator MoveCameraOnDeath(Vector3 endingPos)
       {
           while (_cameraHolder.transform.position != endingPos)
           {
               Vector3.SmoothDamp(_cameraHolder.transform.position, endingPos, ref velocity, 1f);
               yield return null;
           }
       }
       
       #endregion
    }
}
