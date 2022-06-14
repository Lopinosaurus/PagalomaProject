using Photon.Pun;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public class JumpCollisionDetect : MonoBehaviour
    {
        public bool IsColliding => collisions > 0;
        private int collisions;

        private void Start()
        {
            if (!gameObject.GetComponentInParent<PhotonView>().IsMine) Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Player")) collisions++;
        }

        private void OnCollisionExit(Collision other)
        {
            if (!other.gameObject.CompareTag("Player")) collisions -= collisions == 0 ? 0 : 1;
        }
    }
}
