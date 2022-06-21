using System;
using System.Collections;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles.Countdown
{
    public class Countdown : MonoBehaviour
    {
        
        private float countdownValue;
        private float CountdownValue
        {
            get => countdownValue;
            set => countdownValue = value < 0 ? 0 : value;
        }

        public bool isZero => countdownValue == 0;
        public bool isNotZero => countdownValue > 0;
        
        private float countdownMultiplier = 1;

        private void Awake() => StartCoroutine(CountDownManager());

        private IEnumerator CountDownManager()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            
            while (true)
            {
                if (CountdownValue > 0) CountdownValue -= countdownMultiplier * Time.fixedDeltaTime;
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
        
        public void Set(float value) => countdownValue = value < 0 ? 0 : value;
        public void Reset() => Set(0);
        public void SetInfinite() => Set(float.MaxValue);
        private void SetMultiplier(float newMultiplier) => countdownMultiplier = newMultiplier < 0 ? 0 : newMultiplier;
    }
}