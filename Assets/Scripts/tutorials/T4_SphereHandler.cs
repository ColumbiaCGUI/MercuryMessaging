using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MercuryMessaging;

public enum T4_myMethods
{
    UpdateColor = 100
}

public enum T4_myMsgTypes
{
    Color = 1100
}


public class T4_SphereHandler : MmBaseResponder
{
    // // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            GetRelayNode().MmInvoke( new T4_ColorMessage(new Color(1,0,0,1), (MmMethod)T4_myMethods.UpdateColor, (MmMessageType)T4_myMsgTypes.Color, new MmMetadataBlock(MmLevelFilter.Child)));
            GetRelayNode().MmInvoke( new T4_ColorMessage(new Color(0,0,1,1), (MmMethod)T4_myMethods.UpdateColor, (MmMessageType)T4_myMsgTypes.Color, new MmMetadataBlock(MmLevelFilter.Parent)));
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(new Color(0, 1, 0, 1),
                (MmMethod)T4_myMethods.UpdateColor,
                (MmMessageType)T4_myMsgTypes.Color,
                new MmMetadataBlock(MmLevelFilter.Child)));

            GetRelayNode().MmInvoke(
                new T4_ColorMessage(new Color(0, 1, 0, 1),
                (MmMethod)T4_myMethods.UpdateColor,
                (MmMessageType)T4_myMsgTypes.Color,
                new MmMetadataBlock(MmLevelFilter.Parent)));
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(new Color(0, 0, 1, 1),
                (MmMethod)T4_myMethods.UpdateColor,
                (MmMessageType)T4_myMsgTypes.Color,
                new MmMetadataBlock(MmLevelFilter.Child)));

            GetRelayNode().MmInvoke(
                new T4_ColorMessage(new Color(1, 0, 0, 1),
                (MmMethod)T4_myMethods.UpdateColor,
                (MmMessageType)T4_myMsgTypes.Color,
                new MmMetadataBlock(MmLevelFilter.Parent)));
        }

    }
}
