using System.Collections.Generic;
using Photon.Pun;
using UnityEditor;
using UnityEditor.ShaderGraph.Drawing.Inspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace MainGame.PlayerScripts
{
    public class JumpCollisionDetect : MonoBehaviour
    {
        public bool IsColliding => collisions > 0;
        private PlayerMovement _playerMovement;
        private int collisions;

        private void Start()    
        {
            if (!GetComponentInParent<PhotonView>().IsMine)
            {
                Destroy(gameObject);
            }
            else
            {
                _playerMovement = GetComponentInParent<PlayerMovement>();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Player"))
            {
                collisions++;
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (!other.gameObject.CompareTag("Player"))
            {
                collisions -= collisions == 0 ? 0 : 1;
            }
        }
    }
}
