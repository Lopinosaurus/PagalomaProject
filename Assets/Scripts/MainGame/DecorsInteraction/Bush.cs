using MainGame.PlayerScripts;
using UnityEngine;

public class Bush : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerMovement>().isBushMult = true;
            other.gameObject.GetComponent<PlayerMovement>().nbBushes++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null && other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerMovement>().nbBushes--;
            if (other.GetComponent<PlayerMovement>().nbBushes == 0)
            {
                other.gameObject.GetComponent<PlayerMovement>().isBushMult = false;
            }
        }
    }
}
