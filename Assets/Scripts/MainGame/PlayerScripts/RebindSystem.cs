using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

public class RebindSystem : MonoBehaviour
{
    [SerializeField] private InputActionAsset asset;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    

    public void RebindJump()
    {
        rebindingOperation = asset.actionMaps[0].FindAction("Jump").PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithControlsExcluding("Escape")
            .OnMatchWaitForAnother(.1f)
            .OnComplete(operation =>
            {
                rebindingOperation.Dispose();
            })
            .Start();
    }
    
    public void RebindSprint()
    {
        rebindingOperation = asset.actionMaps[0].FindAction("Sprint").PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithControlsExcluding("Escape")
            .OnMatchWaitForAnother(.1f)
            .OnComplete(operation =>
            {
                rebindingOperation.Dispose();
            })
            .Start();
    }
    
    public void RebindCrouch()
    {
        rebindingOperation = asset.actionMaps[0].FindAction("Crouch").PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithControlsExcluding("Escape")
            .OnMatchWaitForAnother(.1f)
            .OnComplete(operation =>
            {
                rebindingOperation.Dispose();
            })
            .Start();
    }
    
    public void RebindKill()
    {
        rebindingOperation = asset.actionMaps[0].FindAction("Kill").PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithControlsExcluding("Escape")
            .OnMatchWaitForAnother(.1f)
            .OnComplete(operation =>
            {
                rebindingOperation.Dispose();
            })
            .Start();
    }
    
    public void RebindClick()
    {
        rebindingOperation = asset.actionMaps[0].FindAction("Click").PerformInteractiveRebinding()
            .WithControlsExcluding("Escape")
            .OnMatchWaitForAnother(.1f)
            .OnComplete(operation =>
            {
                rebindingOperation.Dispose();
            })
            .Start();
    }
    
    public void RebindMove()
    {
        rebindingOperation = asset.actionMaps[0].FindAction("Move").PerformInteractiveRebinding()
            .WithControlsExcluding("Escape")
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(.1f)
            .OnComplete(operation =>
            {
                rebindingOperation.Dispose();
            })
            .Start();
    }
}