using System;
using System.Collections;
using System.Collections.Generic;
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
    private bool _trueIsPlaying;
    public AudioSource footsteps;
    public static readonly List<CharacterController> PlayersCc = new List<CharacterController>();


    private enum FootstepState
    {
        Crouching,
        Walking,
        Sprinting
    }
    
    #region Play FootstepEffect Coroutine
    private IEnumerator _PlayFootstep(AudioClip clip, FootstepState state)
    {
        _trueIsPlaying = true;
        footsteps.clip = clip;
        switch (state)
        {
            case FootstepState.Crouching:
                footsteps.volume = Random.Range(0.1f, 0.2f);
                footsteps.pitch = Random.Range(0.8f, 1.1f);
                footsteps.Play();
                yield return new WaitForSeconds(.8f);
                break;
            case FootstepState.Walking:
                footsteps.volume = Random.Range(0.25f, 0.35f);
                footsteps.pitch = Random.Range(0.8f, 1.1f);
                footsteps.Play();
                yield return new WaitForSeconds(.5f);
                break;
            case FootstepState.Sprinting:
                footsteps.volume = Random.Range(0.25f, 0.35f);
                footsteps.pitch = Random.Range(0.8f, 1.1f);
                footsteps.Play();
                yield return null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, "Bad FootstepState Case");
        }

        _trueIsPlaying = false;
    }
    #endregion

    void Update()
    {
        if (PlayersCc is null)
        {
            Debug.Log("PlayersCC is null !");
            return;
        }
        
        foreach (var characterController in PlayersCc)
        {
            Debug.Log("Footsteps CharacterController : " + characterController);
            // Avoid error on CharacterController destruction
            if (characterController is null)
                return;

            #region Crouching Case

            Vector3 velocity;
            Vector3 velocity1;
            Vector3 velocity2;
            switch (characterController.isGrounded)
            {
                case true when !footsteps.isPlaying && (velocity2 = characterController.velocity).magnitude > 0f && velocity2.magnitude < 1.5f:
                {
                    var soundTaker = Random.Range(1, 2);

                    if (1 == soundTaker)
                    {
                        if (!_trueIsPlaying)
                        {
                            StartCoroutine(_PlayFootstep(dryCrouching, FootstepState.Crouching));
                        }
                    }

                    else
                    {
                        if (!_trueIsPlaying)
                        {
                            StartCoroutine(_PlayFootstep(wetCrouching, FootstepState.Crouching));
                        }
                    }

                    break;
                }
                case true when (velocity1 = characterController.velocity).magnitude > 0f && velocity1.magnitude < 3f:
                {
                    var soundTaker = Random.Range(1, 2);


                    if (1 == soundTaker)
                    {
                        if (!_trueIsPlaying)
                        {
                            StartCoroutine(_PlayFootstep(dryWalking, FootstepState.Walking));
                        }
                    }

                    else
                    {
                        if (!_trueIsPlaying)
                        {
                            StartCoroutine(_PlayFootstep(wetWalking, FootstepState.Walking));
                        }
                    }

                    break;
                }
                case true when !footsteps.isPlaying && (velocity = characterController.velocity).magnitude > 0f && velocity.magnitude < 6f:
                {
                    var soundTaker = Random.Range(1, 2);

                    if (1 == soundTaker)
                    {
                        if (!_trueIsPlaying)
                        {
                            StartCoroutine(_PlayFootstep(drySprinting, FootstepState.Sprinting));
                        }
                    }

                    else
                    {
                        if (!_trueIsPlaying)
                        {
                            StartCoroutine(_PlayFootstep(wetSprinting, FootstepState.Sprinting));
                        }
                    }

                    break;
                }
            }

            #endregion
        }
    }
}
