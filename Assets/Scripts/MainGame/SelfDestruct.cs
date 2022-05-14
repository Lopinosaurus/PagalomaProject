using System;
using UnityEngine;

namespace MainGame
{
    public class SelfDestruct : MonoBehaviour
    {
        [Range(0, 180)]
        public float timeBeforeDeath = 5;

        private void Awake() => Destroy(gameObject, timeBeforeDeath);
    }
}
