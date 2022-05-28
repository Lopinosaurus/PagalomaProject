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
    [SerializeField] private CharacterController characterController;
    [SerializeField] private AudioSource plyAudioSource;
    [SerializeField, Range(.1f, 10)] private float maxDistance;
    [SerializeField] private float playerDistanceCounter;
    [SerializeField] private Vector3 velocity3D;
    [SerializeField] private Vector2 velocity2Draw;
    private Animator animator;
    private PlayerAnimation playerAnimation;

    void Start()
    {
        playerDistanceCounter = 0;
        plyAudioSource.playOnAwake = false;
        plyAudioSource.volume = .2f;
        animator = GetComponent<Animator>();
        playerAnimation = GetComponent<PlayerAnimation>();
    }
    
     void FixedUpdate()
     {
         velocity2Draw = new Vector2
         {
             x = animator.GetFloat(playerAnimation._velocityXHash),
             y = animator.GetFloat(playerAnimation._velocityZHash)
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
