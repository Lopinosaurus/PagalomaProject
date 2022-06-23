using System.Collections;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles.Countdown
{
    public class Countdown : MonoBehaviour
    {
        [Range(0, 60), SerializeField] private float _countdownValue;

        internal float CountdownValue
        {
            get => _countdownValue;
            set => _countdownValue = value < 0 ? 0 : value;
        }

        public bool IsZero => _countdownValue == 0;
        public bool IsNotZero => !IsZero;
        
        private float _countdownMultiplier = 1;

        private void Awake() => StartCoroutine(CountDownManager());

        private IEnumerator CountDownManager()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            
            while (true)
            {
                if (CountdownValue > 0) CountdownValue -= _countdownMultiplier * Time.fixedDeltaTime;
                yield return waitForFixedUpdate;
            }
        }

        // Power timer (how long does it stay active ?)
        public void Pause() => SetMultiplier(0);

        public void Resume(float delayInSeconds = 0)
        {
            if (0 == delayInSeconds) SetMultiplier(1);
            else StartCoroutine(ResumeWithSeconds(delayInSeconds));
        }

        private IEnumerator ResumeWithSeconds(float delayInSeconds)
        {
            yield return new WaitForSeconds(delayInSeconds);
            Resume();
        }
        
        public void Set(float value) => _countdownValue = value < 0 ? 0 : value;
        public void Reset() => Set(0);
        public void SetInfinite() => Set(float.MaxValue);
        private void SetMultiplier(float newMultiplier) => _countdownMultiplier = newMultiplier < 0 ? 0 : newMultiplier;
    }
}