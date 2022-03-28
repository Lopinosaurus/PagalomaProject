using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class debug : MonoBehaviour
{
    private Animator _animator;
    public Animator Animator => _animator;
    
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (null == _animator)
        {
            throw new Exception("NO FUCKING ANIMATOR");
        }
        else
        {
            Debug.Log("THERE IS A FUCKING ANIMATOR");
        }
    }
}
