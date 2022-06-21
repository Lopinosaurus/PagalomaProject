using MainGame.PlayerScripts;
using UnityEngine;

public class Bush : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other is CharacterController && other.CompareTag("Player"))
        {
            other.gameObject.GetComponentInParent<PlayerMovement>().isBushSlowingPlayer = true;
            other.gameObject.GetComponentInParent<PlayerMovement>().nbBushes++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null && other is CharacterController  && other.CompareTag("Player"))
        {
            other.gameObject.GetComponentInParent<PlayerMovement>().nbBushes--;
            if (other.gameObject.GetComponentInParent<PlayerMovement>().nbBushes == 0)
            {
                other.gameObject.GetComponentInParent<PlayerMovement>().isBushSlowingPlayer = false;
            }
        }
    }
}
