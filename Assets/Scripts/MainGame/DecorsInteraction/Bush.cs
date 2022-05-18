using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.PlayerScripts;
using Photon.Realtime;
using UnityEngine;

public class Bush : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerMovement>().currentSpeedMult = 0.4f;
            other.GetComponent<PlayerMovement>().nbBushes++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerMovement>().nbBushes--;
            if (other.GetComponent<PlayerMovement>().nbBushes == 0)
            {
                other.GetComponent<PlayerMovement>().currentSpeedMult = 1f;
            }
        }
    }
}
