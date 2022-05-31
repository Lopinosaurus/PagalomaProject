using UnityEngine.Rendering.PostProcessing;
using UnityEngine;

public class PostProcessVolumeSpawner : MonoBehaviour
{
    private PostProcessVolume _postProcessVolume;
    public float timer = 30;
    private float _timer;

    private void Start()
    {
        _postProcessVolume = GetComponentInChildren<PostProcessVolume>();
        Destroy(gameObject, timer);
        _timer = timer;
    }

    private void FixedUpdate()
    {
        _postProcessVolume.weight = _timer / timer;
        _timer -= Time.fixedDeltaTime;
    }
}
