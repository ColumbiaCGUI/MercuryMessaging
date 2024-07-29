using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool pathOn = false;

    // public bool signalingOn = false;

    void Update()
    {
        if(OVRInput.GetDown(OVRInput.RawButton.X))
        {
            pathOn = !pathOn;
        }
    }
}
