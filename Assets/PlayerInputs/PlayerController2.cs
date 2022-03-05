using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2 : MonoBehaviour
{
    PlayerControls controls;
    Vector2 move;
    public float speed = 10;
    
    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => move = Vector2.zero;
    }
    
    private void OnEnable()
    {
        controls.Player.Enable();
    }
    private void OnDisable()
    {    
        controls.Player.Disable();
    }

    private void SendMessage()
    {
        Debug.Log("Crouch key pressed");
    }
}
