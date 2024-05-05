using System;
using System.Collections;
using System.Collections.Generic;
using MercuryMessaging;
using UnityEngine;
using UnityEngine.InputSystem;


public enum CandleMessageMethod
{
    UpdateLit = 100,
}

public enum CandleMessageType
{
    Lit = 1100,
}
public class ButtonHandler : MmBaseResponder
{
    [SerializeField] private InputActionReference candleAction;
    
    public override void MmInvoke(MmMessage message)
    {

        var type = message.MmMethod;

        switch (type)
        {
            case ((MmMethod)CandleMessageMethod.UpdateLit):
                bool lit = ((CandleMessage)message).interacted;
                break;
            default:
                base.MmInvoke(message);
                break;
        }
    }
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
        Debug.Log("Candle Interacted");
        GetRelayNode().MmInvoke(
            new CandleMessage(true,
                (MmMethod)CandleMessageMethod.UpdateLit,
                (MmMessageType)CandleMessageType.Lit,
                new MmMetadataBlock(MmLevelFilter.Child)));
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
