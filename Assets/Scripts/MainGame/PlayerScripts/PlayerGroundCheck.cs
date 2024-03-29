using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public class PlayerGroundCheck : MonoBehaviour
    {
        private PlayerMovement _playerMovement;
        [SerializeField] private LayerMask characterMask;
        private int _characterMaskValue = 7;
        private int _count;

        private void Awake()
        {
            _playerMovement = GetComponentInParent<PlayerMovement>();
            _characterMaskValue = (int) Mathf.Log(characterMask.value, 2);
            if (!GetComponentInParent<PhotonView>().IsMine) Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (null == collision || collision.gameObject.layer == _characterMaskValue) return;
            _count++;
            _playerMovement.isSphereGrounded = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            if (null == collision || collision.gameObject.layer == _characterMaskValue) return;
            _count--;
            if (_count > 0) return;
            _playerMovement.isSphereGrounded = false;
        }
    }
}