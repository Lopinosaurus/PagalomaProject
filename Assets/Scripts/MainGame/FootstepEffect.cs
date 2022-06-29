using MainGame.PlayerScripts;
using Photon.Pun;
using UnityEngine;

namespace MainGame
{
    public class FootstepEffect : MonoBehaviour
    {
        private float _velocityMagnitude;
        [SerializeField] private AudioClip dryFootstep;
        [SerializeField] private AudioSource plyAudioSource;
        [SerializeField, Range(.1f, 10)] private float maxDistance;
        public float MaxDistance => maxDistance;
        private PlayerAnimation _playerAnimation;
        private CharacterController _characterController;
        private float PlayerDistanceCounter { get; set; }

        private void Start()
        {
            plyAudioSource.playOnAwake = false;
            plyAudioSource.volume = GetComponent<PhotonView>().IsMine ? 0.05f : 0.11f;
            _playerAnimation = GetComponent<PlayerAnimation>();
            _characterController = GetComponent<CharacterController>();
        }

        private void FixedUpdate()
        {
            _velocityMagnitude = _playerAnimation.Velocity;
            PlayerDistanceCounter += _velocityMagnitude * Time.fixedDeltaTime;
            if (PlayerDistanceCounter >= maxDistance)
            {
                PlayerDistanceCounter %= maxDistance;
                plyAudioSource.clip = dryFootstep;
             
                if (_characterController.isGrounded)
                {
                    plyAudioSource.Play();
                }
            }
         
            // Yes, it's realistic.
            if (0 == _velocityMagnitude)
            {
                PlayerDistanceCounter = Mathf.Lerp(PlayerDistanceCounter, 0, Time.fixedDeltaTime);
            }
        }
    }
}
