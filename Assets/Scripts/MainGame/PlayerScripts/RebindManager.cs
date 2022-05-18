using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;

public class RebindManager : MonoBehaviour
{
    public static PlayerControls inputActions;

    private void Awake()
    {
        if (inputActions == null)
            inputActions = new PlayerControls();
    }

    public static void StartRebind(string actionName, int index, Text status)
    {
        InputAction action = inputActions.asset.FindAction(actionName);
        if (action == null || action.bindings.Count <= index)
        {
            Debug.LogWarning("Could not find action or binding required !");
            return;
        }

        if (action.bindings[index].isComposite)
        {
            // Select first composite because we do not want to change main part (Title) of the composite
            var firstIndex = index + 1;
            if (firstIndex < action.bindings.Count && action.bindings[firstIndex].isComposite)
                DoRebind(action, index, status, true);
        }
        else 
            DoRebind(action, index, status, false);
    }
    
    private static void DoRebind(InputAction action, int index, Text status, bool areComposite)
    {
        if (action == null || index < 0)
            return;

        status.text = $"Press {action.expectedControlType}";
        action.Disable();
        var rebind = action.PerformInteractiveRebinding(index);
        rebind.OnComplete(operation =>
        {
            action.Enable();
            // Remove from RAM allocation to avoid crashes
            operation.Dispose();

            if (areComposite)
            {
                var nextIndex = index + 1;
                if (nextIndex < action.bindings.Count && action.bindings[nextIndex].isComposite)
                    DoRebind(action, nextIndex, status, areComposite);
            }
        });

        rebind.OnCancel(operation =>
        {
            action.Enable();
            // Same RAM removal
            operation.Dispose();
        });

        // Start rebinding process
        rebind.Start();
    }
}
