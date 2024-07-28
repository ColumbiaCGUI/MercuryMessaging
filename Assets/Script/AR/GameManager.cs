using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool pathOn = false;

    void Update()
    {
        if(OVRInput.GetDown(OVRInput.RawButton.X))
        {
            pathOn = !pathOn;
        }
    }
}
