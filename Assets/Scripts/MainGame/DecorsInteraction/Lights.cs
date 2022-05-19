using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lights : MonoBehaviour
{
    public Light light;
    public DayNightCycle DNC;

    public void Start()
    {
        DNC = GameObject.FindGameObjectWithTag("DayNightCycle").GetComponent<DayNightCycle>();
        light = this.GetComponent<Light>();
    }

    public void Update()
    {
        light.intensity = -8f * DNC.time * (1f - DNC.time) + 2f;
    }
}
