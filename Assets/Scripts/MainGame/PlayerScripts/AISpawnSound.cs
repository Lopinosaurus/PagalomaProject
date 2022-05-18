using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawnSound : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private AudioSource audioSource;
    private bool registeredIASpawn = false;

    void Update()
    {
        if (playerController.IaAlreadySpawned && !registeredIASpawn)
        {
            registeredIASpawn = true;
            audioSource.Play();
        }
    }
}
