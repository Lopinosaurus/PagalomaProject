using UnityEngine;

namespace MainGame.DecorsInteraction
{
    public class Door : MonoBehaviour
    {
        public GameObject message;
        private void OnTriggerEnter(Collider other)
        {
            other.GetComponent<PlayerInteraction>().NearDoor(message, gameObject, true);
        }
        private void OnTriggerExit(Collider other)
        {
            other.GetComponent<PlayerInteraction>().NearDoor(message, gameObject, false);
        }
    }
}
