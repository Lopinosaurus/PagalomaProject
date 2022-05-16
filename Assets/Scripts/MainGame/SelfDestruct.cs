using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    private void Awake()
    {
        Destroy(gameObject, 5);
    }
}
