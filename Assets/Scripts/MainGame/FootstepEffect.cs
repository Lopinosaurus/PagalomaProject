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
    [SerializeField] private Vector2 velocity2Draw;
    private PlayerAnimation playerAnimation;

    void Start()
    {
        playerDistanceCounter = 0;
        plyAudioSource.playOnAwake = false;
        plyAudioSource.volume = .11f;
        playerAnimation = GetComponent<PlayerAnimation>();
    }
    
     void FixedUpdate()
     {
         velocity2Draw = new Vector2
         {
             x = playerAnimation.velocityX,
             y = playerAnimation.velocityZ
         };
         
         velocityMagnitude = velocity2Draw.magnitude;
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
