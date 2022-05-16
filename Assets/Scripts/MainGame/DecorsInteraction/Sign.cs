using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<PlayerInteraction>().NearSign(true);
    }
    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<PlayerInteraction>().NearSign(false);
    }
}
