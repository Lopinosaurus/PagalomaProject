using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.PlayerScripts;
using UnityEngine;
using Random = UnityEngine.Random;

public class FootstepEffect : MonoBehaviour
{
    
    [SerializeField] AudioClip dryCrouching;
    [SerializeField] AudioClip dryWalking;
    [SerializeField] AudioClip drySprinting;
    [SerializeField] AudioClip wetCrouching;
    [SerializeField] AudioClip wetWalking;
    [SerializeField] AudioClip wetSprinting;
    private bool trueIsPlaying;
    public static List<(CharacterController, AudioSource)> PlayersCc = new List<(CharacterController, AudioSource)>();


    private enum FootstepState
    {
        Crouching,
        Walking,
        Sprinting
    }
    
    #region Play FootstepEffect Coroutine
    private IEnumerator _PlayFootstep(AudioClip clip, FootstepState state, AudioSource plyFoot)
    {
        trueIsPlaying = true;
        plyFoot.clip = clip;
        plyFoot.spatialBlend = 1;
        plyFoot.minDistance = 25;
        plyFoot.maxDistance = 100;
        switch (state)
        {
            case FootstepState.Crouching:
                plyFoot.volume = Random.Range(.03f, .08f);
                plyFoot.pitch = Random.Range(.8f, 1.1f);
                plyFoot.Play();
                yield return new WaitForSeconds(.8f);
                break;
            case FootstepState.Walking:
                plyFoot.volume = Random.Range(.06f, .08f);
                plyFoot.pitch = Random.Range(.8f, 1.1f);
                plyFoot.Play();
                yield return new WaitForSeconds(.5f);
                break;
            case FootstepState.Sprinting:
                plyFoot.volume = Random.Range(.06f, .08f);
                plyFoot.pitch = Random.Range(.8f, 1.1f);
                plyFoot.Play();
                yield return null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, "Bad FootstepState Case");
        }

        trueIsPlaying = false;
    }
    #endregion

    void FixedUpdate()
    {
        if (PlayersCc is null)
        {
            Debug.Log("PlayersCC is null !");
            return;
        }

        // Item 1 = CharacterController, Item 2 = AudioSource
        foreach (var playerData in PlayersCc)
        {
            CharacterController characterController = playerData.Item1;
            AudioSource playerAS = playerData.Item2;
            Debug.Log("Character Controller : " + characterController);
            // Avoid fatal error on destruction
            if (characterController is null || characterController == null)
                return;
            
            switch (characterController.gameObject.GetComponent<PlayerMovement>().grounded)
            {
                #region Sprinting Case
                case true when !playerAS.isPlaying 
                               && characterController.velocity.magnitude > 0f 
                               && characterController.velocity.magnitude < 6f:
                {
                    var soundTaker = Random.Range(1, 2);

                    if (1 == soundTaker)
                    {
                        if (!trueIsPlaying)
                        {
                            StartCoroutine(_PlayFootstep(drySprinting, FootstepState.Sprinting, playerAS));
                        }
                    }

                    else
                    {
                        if (!trueIsPlaying)
                        {
                            StartCoroutine(_PlayFootstep(wetSprinting, FootstepState.Sprinting, playerAS));
                        }
                    }

                    break;
                    
                }
                #endregion
                
                #region Walking Case
                case true when characterController.velocity.magnitude > 1f 
                               && !playerAS.isPlaying
                               && characterController.velocity.magnitude < 3f:
                {
                    
                    Debug.Log("Walking case !");
                    Debug.Log("Velocity : " + characterController.velocity.magnitude);
                    var soundTaker = Random.Range(1, 2);


                    if (1 == soundTaker)
                    {
                        if (!trueIsPlaying)
                        {
                            StartCoroutine(_PlayFootstep(dryWalking, FootstepState.Walking, playerAS));
                        }
                    }

                    else
                    {
                        if (!trueIsPlaying)
                        {
                            StartCoroutine(_PlayFootstep(wetWalking, FootstepState.Walking, playerAS));
                        }
                    }

                    break;
                }
                #endregion
                
                #region Crouching Case
                case true when !playerAS.isPlaying 
                               && characterController.velocity.magnitude > 0f 
                               && characterController.velocity.magnitude < 1.5f:
                {
                    var soundTaker = Random.Range(1, 2);
                
                    Debug.Log("Crouching case !");
                    Debug.Log("Velocity : " + characterController.velocity.magnitude);
                    if (1 == soundTaker)
                    {
                        if (!trueIsPlaying)
                        {
                            StartCoroutine(_PlayFootstep(dryCrouching, FootstepState.Crouching, playerAS));
                        }
                        
                    }
                
                    else
                    {
                        if (!trueIsPlaying)
                        {
                            StartCoroutine(_PlayFootstep(wetCrouching, FootstepState.Crouching, playerAS));
                        }
                        
                    }
                
                    break;
                }
                #endregion
                
            }

                
        }
    }
}
