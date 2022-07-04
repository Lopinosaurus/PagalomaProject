using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float timer = 5;

    private void Start() => Destroy(gameObject, timer);
}
