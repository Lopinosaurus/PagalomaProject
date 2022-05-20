using UnityEngine;

namespace MainGame.DecorsInteraction
{
    public class Sign : MonoBehaviour
    {
        private PlayerInteraction playerInteraction;
        private void OnTriggerEnter(Collider other)
        {
            playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null) playerInteraction.NearSign(true);
        }
        private void OnTriggerExit(Collider other)
        {
            playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null) playerInteraction.NearSign(false);
        }
    }
}
