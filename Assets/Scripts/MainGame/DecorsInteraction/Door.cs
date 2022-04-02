using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject message;

    private void Start()
    {
    }
    
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<PlayerInteraction>().NearDoor(message, gameObject);
    }
    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<PlayerInteraction>().FarDoor(message, gameObject);
    }
}
