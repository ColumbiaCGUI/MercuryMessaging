using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapController : MonoBehaviour
{
    [SerializeField] private InputActionReference mapAction;

    public GameObject map;

    private bool isMapOpen = false;

    void Awake()
    {
        // Enable the action
        mapAction.action.Enable();
        mapAction.action.performed += InteractMap;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void Start()
    {
        map.SetActive(false);
        isMapOpen = false;
    }

    private void OnDestroy()
    {
        mapAction.action.Disable();
        mapAction.action.performed -= InteractMap;
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Disconnected)
        {
            mapAction.action.Disable();
            mapAction.action.performed -= InteractMap;
        }
        else if (change == InputDeviceChange.Reconnected)
        {
            mapAction.action.Enable();
            mapAction.action.performed += InteractMap;
        }
    }

    private void InteractMap(InputAction.CallbackContext context)
    {
        Debug.Log("Map button pressed");
        if (isMapOpen)
        {
            map.SetActive(false);
            isMapOpen = false;
        }
        else
        {
            map.SetActive(true);
            isMapOpen = true;
        }
    }
}
