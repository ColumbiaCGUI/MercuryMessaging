using System;
using System.Collections;
using System.Collections.Generic;
using MercuryMessaging;
using UnityEngine;
using UnityEngine.InputSystem;


public class GoNoGoButtonHandler : MonoBehaviour
{
    [SerializeField] private InputActionReference candleAction;
    [SerializeField] private Task4GameController gameController;
    void Awake()
    {
        // Enable the action
        candleAction.action.Enable();
        candleAction.action.performed += InteractCandle;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDestroy()
    {
        candleAction.action.Disable();
        candleAction.action.performed -= InteractCandle;
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    
    private void InteractCandle(InputAction.CallbackContext context)
    {
        Debug.Log("Button pressed.");
        gameController.isButtonPressed = true;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Disconnected:
                candleAction.action.Disable();
                candleAction.action.performed -= InteractCandle;
                break;
            case InputDeviceChange.Reconnected:
                candleAction.action.Enable();
                candleAction.action.performed += InteractCandle;
                break;
        }
    }
}
