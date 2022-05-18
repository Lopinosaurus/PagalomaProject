using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindSystem : MonoBehaviour
{
    [Header("System Informations")]
    [SerializeField] private InputActionReference inputActionReference;
    [SerializeField] private bool excludeMouseControls = true;
    [SerializeField] private int selectedBind;
    [SerializeField] private InputBinding.DisplayStringOptions displayStringOptions;

    [Header("Binding Informations")] 
    [SerializeField] private InputBinding inputBinding;
    private int bindIndex;
    private string bindNameTag;
    
    [Header("UI Manager")] 
    [SerializeField] private Button rebindButton;
    [SerializeField] private Button resetControlButton;
    [SerializeField] private TMP_Text bindText;
    [SerializeField] Text rebindText;


    private void OnEnable()
    {
        rebindButton.onClick.AddListener(() => DoRebind());
        resetControlButton.onClick.AddListener(() => DoReset());

        if (inputActionReference != null)
        {
            GetBindInfo();
            UpdateUI();
        }
    }
    
    // Triggered when something changes on the inspector
    private void OnValidate()
    {
        if (inputActionReference is null)
            return;
        
        GetBindInfo();
        UpdateUI();
    }

    private void GetBindInfo()
    {
        if (inputActionReference != null)
            bindNameTag = inputActionReference.action.name;

        if (inputActionReference.action.bindings.Count > selectedBind)
        {
            inputBinding = inputActionReference.action.bindings[selectedBind];
            bindIndex = selectedBind;
        }
    }

    private void UpdateUI()
    {
        if (bindText != null)
            bindText.text = bindNameTag;

        if (rebindText != null)
        {
            if (Application.isPlaying)
            {

            }

            else
                rebindText.text = inputActionReference.action.GetBindingDisplayString();
        }
    }

    private void DoRebind()
    {
        RebindManager.StartRebind(bindNameTag, bindIndex, rebindText);
    }

    private void DoReset()
    {
        
    }
}