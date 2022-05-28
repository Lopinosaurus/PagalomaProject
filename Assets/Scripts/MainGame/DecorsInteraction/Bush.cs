using MainGame.PlayerScripts;
using UnityEngine;

public class Bush : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerMovement>().isBushMult = true;
            other.GetComponent<PlayerMovement>().nbBushes++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerMovement>().nbBushes--;
            if (other.GetComponent<PlayerMovement>().nbBushes == 0)
            {
                other.GetComponent<PlayerMovement>().isBushMult = false;
            }
        }
    }
}
