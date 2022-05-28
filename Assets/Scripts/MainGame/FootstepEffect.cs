using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.PlayerScripts;
using UnityEngine;
using Random = UnityEngine.Random;

public class FootstepEffect : MonoBehaviour
{
    [SerializeField] private float velocityMagnitude;
    [SerializeField] private AudioClip dryFootstep;
    [SerializeField] private AudioSource plyAudioSource;
    [SerializeField, Range(.1f, 10)] private float maxDistance;
    [SerializeField] private float playerDistanceCounter;
    private PlayerAnimation playerAnim;

    void Start()
    {
        playerDistanceCounter = 0;
        playerAnim = GetComponent<PlayerAnimation>();
        plyAudioSource.playOnAwake = false;
        plyAudioSource.volume = .2f;
    }
    
    void FixedUpdate()
    {
        velocityMagnitude = playerAnim.Velocity2DMagnitude;
        
        playerDistanceCounter += velocityMagnitude * Time.fixedDeltaTime;
        if (playerDistanceCounter >= maxDistance)
        {
            playerDistanceCounter = 0;
            plyAudioSource.clip = dryFootstep;
            plyAudioSource.Play();
        }
        
        // Yes, it's realistic.
        if (0 == velocityMagnitude)
        {
            plyAudioSource.Stop();    
        }
        
    }
}
