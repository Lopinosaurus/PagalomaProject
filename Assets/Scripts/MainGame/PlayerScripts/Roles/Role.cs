using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class Role : PlayerController
    {
       public bool isAlive;
       public string username;
       public string color;
       public Role vote;

       public Role(string username, string color)
       {
           this.isAlive = true;
           this.username = username;
           this.color = color;
           this.vote = null;
       }

       #region Gameplay methods

       private IEnumerator coroutine;
       public void Die()
       {
           // Disable every control
           _playerMovement.PlayerControls.Disable();

           // Freezes the player & Disable collision
           _playerLook.enabled = false;
           _playerMovement.enabled = false;
           _characterController.enabled = false;
           _characterController.detectCollisions = false;

           // Moves the camera above the player
           Vector3 endingPos = cameraHolder.transform.position;

           Vector3 rayOrigin = _camera.WorldToViewportPoint(new Vector3(0.5f,0.5f,0));
           if (Physics.Raycast(rayOrigin, Vector3.up, out RaycastHit hitInfo, 5.1f))
           {
               endingPos = hitInfo.point;
           }

           coroutine = MoveCameraOnDeath(endingPos);
           StartCoroutine(coroutine);
       }

       private IEnumerator MoveCameraOnDeath(Vector3 endingPos)
       {
           Vector3 velocity = default;
           Vector3.SmoothDamp(cameraHolder.transform.position, endingPos, ref velocity, 0.01f);
           yield return new WaitUntil(() => cameraHolder.transform.position == endingPos);
       }

       #endregion

    }
}
