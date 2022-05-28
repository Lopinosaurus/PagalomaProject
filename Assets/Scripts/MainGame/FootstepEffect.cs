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
    [SerializeField] private CharacterController characterController;
    [SerializeField, Range(.1f, 10)] private float maxDistance;
    [SerializeField] private float playerDistanceCounter;
    [SerializeField] private int footstepCounter;
    public static List<(CharacterController, AudioSource)> PlayersCc = new List<(CharacterController, AudioSource)>();
    private PlayerAnimation playerAnim;

    void Start()
    {
        footstepCounter = 0;
        playerDistanceCounter = 0;
        playerAnim = GetComponent<PlayerAnimation>();
        plyAudioSource.playOnAwake = false;
        plyAudioSource.volume = .2f;
    }
    
    void FixedUpdate()
    {
        // if (PlayersCc is null)
        // {
        //     Debug.Log("PlayersCC is null !");
        //     return;
        // }
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
