using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace MainGame.Helpers
{
    public class FXManager : MonoBehaviour
    {
        public static FXManager Instance;
        
        public AudioClip[] clips;
        public PostProcessVolume[] ppvs;
        private List<AudioFX> audioFxs;
        private List<VisualFX> visualFxs;

        private int highestPriority = 0;
        
        private void Awake()
        {
            // Singleton pattern management
            if (!GetComponent<PhotonView>().IsMine || Instance)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public void CreateAudioFX(AudioClip clip, float volume = 1, float pitch = 1, bool loop = false)
        {
            GameObject o = new GameObject($"AudioFX {clip.name} {audioFxs.Count + 1}") {transform = {parent = transform}};
            AudioSource source = o.AddComponent<AudioSource>();
            
            source.name = clip.name;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;
            source.playOnAwake = true;
            source.clip = clip;

            AudioFX audioFX = o.AddComponent<AudioFX>();
            audioFX.source = source;
            audioFX.audioClip = clip;
            audioFxs.Add(audioFX);
        }
        
        public void CreateVisualFX(PostProcessVolume volume, float duration)
        {
            GameObject o = new GameObject($"VisualFX {volume.name} {visualFxs.Count + 1}") {transform = {parent = transform}};
            PostProcessVolume addedVolume = o.AddComponent<PostProcessVolume>();

            addedVolume.isGlobal = false;
            addedVolume.priority = highestPriority++;
            addedVolume.profile = volume.profile;
            addedVolume.weight = 0;

            VisualFX visualFX = o.AddComponent<VisualFX>();
            visualFX.volume = addedVolume;
            visualFX.duration = duration;
            visualFxs.Add(visualFX);
        }
    }
}