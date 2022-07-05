using UnityEngine;

namespace MainGame.DecorsInteraction
{
    public class Sign : MonoBehaviour
    {
        private PlayerInteraction _playerInteraction;
        private void OnTriggerEnter(Collider other)
        {
            _playerInteraction = other.GetComponent<PlayerInteraction>();
            if (_playerInteraction) _playerInteraction.NearSign(true);
        }
        private void OnTriggerExit(Collider other)
        {
            _playerInteraction = other.GetComponent<PlayerInteraction>();
            if (_playerInteraction) _playerInteraction.NearSign(false);
        }
    }
}
