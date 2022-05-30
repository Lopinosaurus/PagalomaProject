using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.PlayerScripts;
using UnityEngine;
using Random = UnityEngine.Random;

public class FootstepEffect : MonoBehaviour
{
    private float velocityMagnitude;
    [SerializeField] private AudioClip dryFootstep;
    [SerializeField] private AudioSource plyAudioSource;
    [SerializeField, Range(.1f, 10)] private float maxDistance;
    private float playerDistanceCounter;
    private Vector2 velocity2Draw;
    private PlayerAnimation playerAnimation;
    public float PlayerDistanceCounter => playerDistanceCounter;

    void Start()
    {
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
         playerDistanceCounter = PlayerDistanceCounter + velocityMagnitude * Time.fixedDeltaTime;
         if (PlayerDistanceCounter >= maxDistance)
         {
             playerDistanceCounter = 0;
             plyAudioSource.clip = dryFootstep;
             plyAudioSource.Play();
         }
         
         // Yes, it's realistic.
         if (0 == velocityMagnitude)
         {
             playerDistanceCounter = 0;
             plyAudioSource.Stop();    
         }
     }
}
