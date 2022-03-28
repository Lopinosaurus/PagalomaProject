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
        public string roleName;
        public bool isAlive = true;
        public string username;
        public string color;
        public Role vote;
        
        [SerializeField] private bool selfKill = false;
        [SerializeField] private bool kill = false;

        // Controls
        [SerializeField] private GameObject cameraHolder;
        public PlayerControls PlayerControls;
        private PlayerMovement _playerMovement;
        private PlayerController _playerController;
        private PlayerLook _playerLook;
        private PlayerAnimation _playerAnimation;
        private CharacterController _characterController;
        private Camera _cam;
        
        // Die variables
        private const float MaxDeathCamDistance = 5.0f;
        
        #endregion

       #region Unity Methods

       private void Awake()
       {
           _playerController = GetComponent<PlayerController>();
           _playerLook = GetComponent<PlayerLook>();
           _playerAnimation = GetComponent<PlayerAnimation>();
           _playerMovement = GetComponent<PlayerMovement>();
           _characterController = GetComponent<CharacterController>();
           _cam = cameraHolder.GetComponentInChildren<Camera>();
       }

       private void Start()
       {
           PlayerControls = _playerController.PlayerControls;
           
           PlayerControls.Player.Die.started += ctx => selfKill = ctx.ReadValueAsButton();
           PlayerControls.Player.Kill.started += ctx => kill = ctx.ReadValueAsButton();
           PlayerControls.Player.Kill.canceled  += ctx => kill = ctx.ReadValueAsButton();
       }

       private void LateUpdate()
       {
           if (selfKill && isAlive) Die();
           if (kill) KillTarget();
       }

       #endregion
       
       #region Gameplay methods

       public virtual void KillTarget()
       {
           Debug.Log("E pressed but you are not a Werewolf");
       }
       public void Die()
       {
           // Disable components & gameplay variables
           PlayerControls.Disable();
           _characterController.detectCollisions = false;
           _playerController.enabled = false;
           isAlive = false;
           
           // Initial camera position
           Vector3 startingPos = cameraHolder.transform.position;
           Quaternion startingRot = cameraHolder.transform.rotation;
           Vector3 endingPos = new Vector3
           {
               x = startingPos.x,
               y = startingPos.y + MaxDeathCamDistance,
               z = startingPos.z
           };
          
           Debug.Log("startingRot is:" + startingRot);

           // Final camera position
           if (Physics.Raycast(startingPos, Vector3.up, out RaycastHit hitInfo, MaxDeathCamDistance))
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
           
           Debug.Log("endingRot is:" + endingRot);

           // Start camera animation
           _playerAnimation.EnableDeathAppearance();
           StartCoroutine(MoveCamHolder(endingPos, endingRot));
       }

       private IEnumerator MoveCamHolder(Vector3 endingPos, Quaternion endingRot)
       {
           while (cameraHolder.transform.position != endingPos)
           {
               Vector3 position = cameraHolder.transform.position;
               Quaternion rotation = cameraHolder.transform.localRotation;

               position = Vector3.Slerp(position, endingPos, 0.02f);
               rotation = Quaternion.Slerp(rotation, endingRot, 0.05f);
               
               cameraHolder.transform.position = position;
               cameraHolder.transform.localRotation = rotation;
               yield return null;
           }
       }

       #endregion
    }
}
