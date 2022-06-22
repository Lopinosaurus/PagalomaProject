using UnityEngine;

public class Lights : MonoBehaviour
{
    private Light _light;
    private DayNightCycle _dnc;

    public void Awake()
    {
        _light = GetComponent<Light>();
    }

    private void Start()
    {
        var dayNightCycleGameObject = GameObject.FindGameObjectWithTag("DayNightCycle");
        if (dayNightCycleGameObject) _dnc = dayNightCycleGameObject.GetComponent<DayNightCycle>();
    }

    public void Update()
    {
        if (_dnc) _light.intensity = -8f * _dnc.time * (1f - _dnc.time) + 2f;
    }
}
