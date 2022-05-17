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
        }
        /*
        if (other.tag == "Player")
        {
            PlayerMovement PM = other.GetComponent<PlayerMovement>();
            Debug.Log(PM.currentSpeedMult);
            if (PM.currentMovementType == PlayerMovement.MovementTypes.Crouch)
            {
                PM.currentSpeedMult = 0.8f;
            }
            else if (PM.currentMovementType == PlayerMovement.MovementTypes.Walk)
            {
                PM.currentSpeedMult = 0.6f;
            }
            else if (PM.currentMovementType == PlayerMovement.MovementTypes.Walk)
            {
                PM.currentSpeedMult = 0.7f;
            }
        }*/
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerMovement>().currentSpeedMult = 1f;
        }
    }
}
