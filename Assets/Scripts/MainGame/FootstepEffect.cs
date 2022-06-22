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
        private float _playerDistanceCounter;
        private PlayerAnimation _playerAnimation;
        private PlayerMovement _playerMovement;
        public float PlayerDistanceCounter => _playerDistanceCounter;

        void Start()
        {
            plyAudioSource.playOnAwake = false;
            plyAudioSource.volume = GetComponent<PhotonView>().IsMine ? 0.05f : 0.11f;
            _playerAnimation = GetComponent<PlayerAnimation>();
            _playerMovement = GetComponent<PlayerMovement>();
        }
    
        void FixedUpdate()
        {
            _velocityMagnitude = _playerAnimation.Velocity;
            _playerDistanceCounter = PlayerDistanceCounter + _velocityMagnitude * Time.fixedDeltaTime;
            if (PlayerDistanceCounter >= maxDistance)
            {
                _playerDistanceCounter %= maxDistance;
                plyAudioSource.clip = dryFootstep;
             
                if (_playerMovement.grounded)
                {
                    plyAudioSource.Play();
                }
            }
         
            // Yes, it's realistic.
            if (0 == _velocityMagnitude)
            {
                _playerDistanceCounter = Mathf.Lerp(_playerDistanceCounter, 0, Time.fixedDeltaTime);
            }
        }
    }
}
