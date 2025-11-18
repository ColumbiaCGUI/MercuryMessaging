using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRInit : MonoBehaviour
{
    void Start()
    {
    if (OVRManager.IsPassthroughRecommended())
    {
        // passthroughLayer.enabled = true;

        // Set camera background to transparent
        OVRCameraRig ovrCameraRig = GameObject.Find("OVRCameraRig").GetComponent<OVRCameraRig>();
        var centerCamera = ovrCameraRig.centerEyeAnchor.GetComponent<Camera>();
        centerCamera.clearFlags = CameraClearFlags.SolidColor;
        centerCamera.backgroundColor = Color.clear;

        // Ensure your VR background elements are disabled
    }
    else
    {
        // passthroughLayer.enabled = false;

        // Ensure your VR background elements are enabled
    }
    }
}
