using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace MainGame.Helpers
{
    public abstract class FX : MonoBehaviour {
        public abstract void Stop();

        protected IEnumerator DestroyOnCondition(Func<bool> predicate)
        {
            yield return null;
            yield return new WaitWhile(predicate);
            Destroy(gameObject);
        }
        
        protected IEnumerator DestroyOnTimer(float timer)
        {
            yield return null;
            yield return new WaitForSeconds(timer);
            yield return Fade(0);
            Destroy(gameObject);
        }

        protected abstract IEnumerator Fade(float target);
    }

    public class AudioFX : FX
    {
        public AudioClip audioClip;
        public AudioSource source;

        private void Awake() => StartCoroutine(DestroyOnCondition(() => source.isPlaying));
        public override void Stop() => StartCoroutine(Fade(0));

        protected override IEnumerator Fade(float target)
        {
            float timer = 1;
            
            while (timer > 0 || Mathf.Abs(source.volume - target) > 0.01f)
            {
                timer -= Time.deltaTime;
                source.volume = Mathf.Lerp(source.volume, target, Time.deltaTime * 30);

                yield return null;
            }
            
            Destroy(source.gameObject);
            Destroy(this);
        }
    }

    public class VisualFX : FX
    {
        public PostProcessVolume volume;
        public float duration;

        private void Awake()
        {
            PostProcessVolume a = gameObject.AddComponent<PostProcessVolume>();
            StartCoroutine(Fade(1));

            StartCoroutine(DestroyOnTimer(duration));
        }

        public override void Stop() => StartCoroutine(Fade(0));

        protected override IEnumerator Fade(float target)
        {
            float timer = 1;
            
            while (timer > 0 || Mathf.Abs(volume.weight - target) > 0.01f)
            {
                timer -= Time.deltaTime;
                volume.weight = Mathf.Lerp(volume.weight, target, Time.deltaTime * 20);

                yield return null;
            }
           
            Destroy(volume);
            Destroy(this);
        }
    }
}
