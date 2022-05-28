using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lights : MonoBehaviour
{
    public Light _light;
    public DayNightCycle DNC;

    public void Start()
    {
        DNC = GameObject.FindGameObjectWithTag("DayNightCycle").GetComponent<DayNightCycle>();
        _light = this.GetComponent<Light>();
    }

    public void Update()
    {
        _light.intensity = -8f * DNC.time * (1f - DNC.time) + 2f;
    }
}
