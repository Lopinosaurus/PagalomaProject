using UnityEngine;

namespace MainGame
{
    public class SelfDestruct : MonoBehaviour
    {
        private void Awake()
        {
            Destroy(gameObject, 5);
        }
    }
}
