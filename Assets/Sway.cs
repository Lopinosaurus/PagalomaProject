using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sway : MonoBehaviour
{
    private float _angle;
    [SerializeField, Range(0, 10)] private float smoothTime;
    private Vector3 _offset;
    private const float PI2 = 2 * Mathf.PI;

    private void Awake()
    {
        _offset = transform.localPosition;
    }

    private void FixedUpdate()
    {
        _angle += Time.fixedDeltaTime;
        if (_angle >= PI2) _angle -= PI2;

        Vector3 _ = Vector3.zero;
        transform.localPosition = _offset
                                  + Vector3.SmoothDamp(Vector3.zero, Mathf.Sin(_angle) * Vector3.right, ref _, smoothTime)
                                  + Vector3.SmoothDamp(Vector3.zero, Mathf.Cos(_angle) * Vector3.up, ref _, smoothTime);
    }
}
