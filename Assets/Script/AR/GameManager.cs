using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public bool pathOn = false;

    public InputActionAsset playerInput;

    private InputAction pathAction;

    void Start()
    {
        pathAction = playerInput.FindActionMap("XRI LeftHand Interaction").FindAction("PathSwitch");
        pathAction.Enable();
    }

    void Update()
    {
        if(pathAction.triggered)
        {
            pathOn = !pathOn;
        }
    }
}
