using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.PlayerScripts;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class FootstepEffect : MonoBehaviour
{
    private float velocityMagnitude;
    [SerializeField] private AudioClip dryFootstep;
    [SerializeField] private AudioSource plyAudioSource;
    [SerializeField, Range(.1f, 10)] private float maxDistance;
    public float MaxDistance => maxDistance;
    private float playerDistanceCounter;
    private PlayerAnimation playerAnimation;
    public float PlayerDistanceCounter => playerDistanceCounter;

    void Start()
    {
        plyAudioSource.playOnAwake = false;
        plyAudioSource.volume = GetComponent<PhotonView>().IsMine ? 0.05f : 0.11f;
        playerAnimation = GetComponent<PlayerAnimation>();
    }
    
     void FixedUpdate()
     {
         velocityMagnitude = playerAnimation.velocity;
         playerDistanceCounter = PlayerDistanceCounter + velocityMagnitude * Time.fixedDeltaTime;
         if (PlayerDistanceCounter >= maxDistance)
         {
             playerDistanceCounter %= maxDistance;
             plyAudioSource.clip = dryFootstep;
             plyAudioSource.Play();
         }
         
         // Yes, it's realistic.
         if (0 == velocityMagnitude)
         {
             playerDistanceCounter = Mathf.Lerp(playerDistanceCounter, 0, Time.deltaTime);
             // plyAudioSource.Stop();    
         }
     }
}
