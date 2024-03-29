using MainGame.PlayerScripts;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindInputs : MonoBehaviour
{

    // Rebinding Player Fields
    [SerializeField] private InputActionReference refJump;
    [SerializeField] private PlayerController playerController;
    private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

    // TextMeshPro
    [SerializeField] private TMPro.TextMeshPro bindingKeyDisplay;
    
    // Rebinding Game Objects
    [SerializeField] private GameObject startRebind;
    [SerializeField] private GameObject waitingForInput;

    public void StartRebinding()
    {
        startRebind.SetActive(false);
        waitingForInput.SetActive(true);

        // Swap BindMap to "In UI" mode
        //playerController.PlayerInput.SwitchCurrentActionMap("UI");

        _rebindingOperation = refJump.action.PerformInteractiveRebinding()
            .WithControlsExcluding("Escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(action => RebindComplete())
            .Start();

    }

    private void RebindComplete()
    {
        // Deleting operation from memory
        _rebindingOperation.Dispose();

        startRebind.SetActive(true);
        waitingForInput.SetActive(false);

        // Swap BindMap to "In Game" mode
        //playerController.PlayerInput.SwitchCurrentActionMap("Player");

    }
}
