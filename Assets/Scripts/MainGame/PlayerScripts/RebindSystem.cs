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
    [SerializeField] private Text jumpText;
    [SerializeField] private Text crouchText;
    [SerializeField] private Text sprintText;
    [SerializeField] private Text killText;
    [SerializeField] private Text clickText;

    private void Start()
    {
        // Setting text foreach control
        int bindIndex = asset.actionMaps[0].FindAction("Jump").GetBindingIndexForControl(asset.actionMaps[0].FindAction("Jump").controls[0]);
        jumpText.text = InputControlPath.ToHumanReadableString(
            asset.actionMaps[0].FindAction("Jump").bindings[bindIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
        
        bindIndex = asset.actionMaps[0].FindAction("Crouch").GetBindingIndexForControl(asset.actionMaps[0].FindAction("Crouch").controls[0]);
        crouchText.text = InputControlPath.ToHumanReadableString(
            asset.actionMaps[0].FindAction("Crouch").bindings[bindIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
        
        bindIndex = asset.actionMaps[0].FindAction("Sprint").GetBindingIndexForControl(asset.actionMaps[0].FindAction("Sprint").controls[0]);
        sprintText.text = InputControlPath.ToHumanReadableString(
            asset.actionMaps[0].FindAction("Sprint").bindings[bindIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
        
        bindIndex = asset.actionMaps[0].FindAction("Kill").GetBindingIndexForControl(asset.actionMaps[0].FindAction("Kill").controls[0]);
        killText.text = InputControlPath.ToHumanReadableString(
            asset.actionMaps[0].FindAction("Kill").bindings[bindIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
        
        bindIndex = asset.actionMaps[0].FindAction("Click").GetBindingIndexForControl(asset.actionMaps[0].FindAction("Click").controls[0]);
        clickText.text = InputControlPath.ToHumanReadableString(
            asset.actionMaps[0].FindAction("Click").bindings[bindIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    public void RebindJump()
    {
        jumpText.text = "Enter a key...";
        
        rebindingOperation = asset.actionMaps[0].FindAction("Jump").PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithControlsExcluding("Escape")
            .OnMatchWaitForAnother(.1f)
            .OnComplete(operation =>
            {
                int bindIndex = asset.actionMaps[0].FindAction("Jump").GetBindingIndexForControl(asset.actionMaps[0].FindAction("Jump").controls[0]);
                
                jumpText.text = InputControlPath.ToHumanReadableString(
                    asset.actionMaps[0].FindAction("Jump").bindings[bindIndex].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
                
                rebindingOperation.Dispose();
            })
            .Start();
    }
    
    public void RebindSprint()
    {
        sprintText.text = "Enter a key...";
        
        rebindingOperation = asset.actionMaps[0].FindAction("Sprint").PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithControlsExcluding("Escape")
            .OnMatchWaitForAnother(.1f)
            .OnComplete(operation =>
            {
                int bindIndex = asset.actionMaps[0].FindAction("Sprint").GetBindingIndexForControl(asset.actionMaps[0].FindAction("Sprint").controls[0]);
                
                sprintText.text = InputControlPath.ToHumanReadableString(
                    asset.actionMaps[0].FindAction("Sprint").bindings[bindIndex].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
                
                rebindingOperation.Dispose();
            })
            .Start();
    }
    
    public void RebindCrouch()
    {
        crouchText.text = "Enter a key...";
        
        rebindingOperation = asset.actionMaps[0].FindAction("Crouch").PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithControlsExcluding("Escape")
            .OnMatchWaitForAnother(.1f)
            .OnComplete(operation =>
            {
                int bindIndex = asset.actionMaps[0].FindAction("Crouch").GetBindingIndexForControl(asset.actionMaps[0].FindAction("Crouch").controls[0]);
                
                crouchText.text = InputControlPath.ToHumanReadableString(
                    asset.actionMaps[0].FindAction("Crouch").bindings[bindIndex].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
                
                rebindingOperation.Dispose();
            })
            .Start();
    }
    
    public void RebindKill()
    {
        killText.text = "Enter a key...";
        
        rebindingOperation = asset.actionMaps[0].FindAction("Kill").PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithControlsExcluding("Escape")
            .OnMatchWaitForAnother(.1f)
            .OnComplete(operation =>
            {
                int bindIndex = asset.actionMaps[0].FindAction("Kill").GetBindingIndexForControl(asset.actionMaps[0].FindAction("Kill").controls[0]);
                
                killText.text = InputControlPath.ToHumanReadableString(
                    asset.actionMaps[0].FindAction("Kill").bindings[bindIndex].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
                
                rebindingOperation.Dispose();
            })
            .Start();
    }
    
    public void RebindClick()
    {
        clickText.text = "Enter a key...";
        
        rebindingOperation = asset.actionMaps[0].FindAction("Click").PerformInteractiveRebinding()
            .WithControlsExcluding("Escape")
            .OnMatchWaitForAnother(.1f)
            .OnComplete(operation =>
            {
                int bindIndex = asset.actionMaps[0].FindAction("Click").GetBindingIndexForControl(asset.actionMaps[0].FindAction("Click").controls[0]);
                
                clickText.text = InputControlPath.ToHumanReadableString(
                    asset.actionMaps[0].FindAction("Click").bindings[bindIndex].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
                
                rebindingOperation.Dispose();
            })
            .Start();
    }
}