using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject message;

    private void OnTriggerEnter(Collider other)
    {
        message.SetActive(true);
    }
    private void OnTriggerExit(Collider other)
    {
        message.SetActive(false);
    }
}
