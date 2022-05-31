using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float timer = 5;

    private void Start()
    {
        Destroy(gameObject, timer);
    }
}
