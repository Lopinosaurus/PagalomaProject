using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using Photon.Pun;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject message;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetType() == typeof(CharacterController) && other.GetComponent<PhotonView>().IsMine)
        {
            other.GetComponent<PlayerInteraction>().NearDoor(message, gameObject, true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetType() == typeof(CharacterController) && other.GetComponent<PhotonView>().IsMine)
        {
            other.GetComponent<PlayerInteraction>().NearDoor(message, gameObject, false);
        }
    }
}
