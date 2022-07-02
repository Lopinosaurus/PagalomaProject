using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace MainGame.Helpers
{
    public class PlayerVFX : MonoBehaviour
    {
        public static List<PlayerVFX> PlayerVFXs;
    
        private readonly PostProcessVolume _volume;
        private readonly AudioClip _audioClip;
        private VFX _vfx;
        public readonly float initialTimer;
        private float _timer;
        
        public enum VFX
        {
            AiStun
        }

        private void Awake()
        {
            PlayerVFXs.Add(this);
            GetComponentInParent<AudioSource>().PlayOneShot(_audioClip);
            
            Destroy(this, initialTimer);
        }

        private void OnDestroy() => PlayerVFXs.Remove(this);

        public PlayerVFX(PostProcessVolume volume, VFX VfxType, AudioClip audioClip, float initialTimer)
        {
            _volume = volume;
            _audioClip = audioClip;
            _vfx = VfxType;
            this.initialTimer = _timer = initialTimer;
        }
    
        private void FixedUpdate()
        {
            _volume.weight = _timer / initialTimer;
            _timer -= Time.fixedDeltaTime;
        }
    }
}
