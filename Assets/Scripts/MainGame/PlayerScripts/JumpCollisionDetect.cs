using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public class JumpCollisionDetect : MonoBehaviour
    {
        private int _collisions;
        public bool IsColliding => _collisions > 0;

        private void Start()
        {
            if (!gameObject.GetComponentInParent<PhotonView>().IsMine) Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Player")) _collisions++;
        }

        private void OnCollisionExit(Collision other)
        {
            if (!other.gameObject.CompareTag("Player")) _collisions -= _collisions == 0 ? 0 : 1;
        }
    }
}