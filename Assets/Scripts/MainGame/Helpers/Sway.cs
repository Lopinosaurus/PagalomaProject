using UnityEngine;
using Random = UnityEngine.Random;

namespace MainGame.Helpers
{
    public class Sway : MonoBehaviour
    {
        [SerializeField] [Range(0, 10)] private float speed;
        private float _angle;
        private Vector3 _offset, _goal, _;

        private void Awake()
        {
            _offset = transform.localPosition;
        }

        private void Update()
        {
            Vector3 transformLocalPosition = transform.localPosition;
            if ((transformLocalPosition - _goal).sqrMagnitude > 0.01f)
            {
                transform.localPosition = Vector3.Slerp(transformLocalPosition, _goal, Time.deltaTime * speed);
            }
            else
            {
                _goal = _offset + Vector3.forward * (Mathf.PerlinNoise(Time.time, 0) - 0.5f)
                                + Vector3.right * (Mathf.PerlinNoise(0, Time.time) - 0.5f)
                                + Vector3.up * (Mathf.PerlinNoise(Time.time, Time.time) - 0.5f);

            }
        }
    }
}