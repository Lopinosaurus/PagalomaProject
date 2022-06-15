using UnityEngine;

public class Lights : MonoBehaviour
{
    private Light _light;
    private DayNightCycle DNC;

    public void Awake()
    {
        _light = GetComponent<Light>();
    }

    private void Start()
    {
        var DayNightCycleGameObject = GameObject.FindGameObjectWithTag("DayNightCycle");
        if (DayNightCycleGameObject) DNC = DayNightCycleGameObject.GetComponent<DayNightCycle>();
    }

    public void Update()
    {
        if (DNC) _light.intensity = -8f * DNC.time * (1f - DNC.time) + 2f;
    }
}
