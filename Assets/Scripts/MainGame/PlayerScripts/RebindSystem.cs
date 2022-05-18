using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindSystem : MonoBehaviour
{
    private PlayerInput _playerInput;
    [SerializeField] private GameObject startRebindObject = null;
    [SerializeField] private GameObject waitingForInputObject = null;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;


    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    public void RebindJump()
    {
        startRebindObject.SetActive(false);
        waitingForInputObject.SetActive(true);
        
        rebindingOperation = _playerInput.actions["Jump"].PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithControlsExcluding("Escape")
            .OnMatchWaitForAnother(.1f)
            .OnComplete(operation =>
            {
                rebindingOperation.Dispose();
                startRebindObject.SetActive(true);
                waitingForInputObject.SetActive(false);
            });
    }
}