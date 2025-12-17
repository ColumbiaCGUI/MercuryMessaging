using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;

/// <summary>
/// Tutorial 1: Basic responder that receives string, int, and Initialize messages.
/// Demonstrates how to extend MmBaseResponder and override ReceivedMessage methods.
/// </summary>
public class T1_ChildResponder : MmBaseResponder
{
    /// <summary>
    /// Called when a string message is received.
    /// </summary>
    protected override void ReceivedMessage(MmMessageString message)
    {
        Debug.Log($"[{gameObject.name}] Received string: {message.value}");
    }

    /// <summary>
    /// Called when an int message is received.
    /// </summary>
    protected override void ReceivedMessage(MmMessageInt message)
    {
        Debug.Log($"[{gameObject.name}] Received int: {message.value}");
    }

    /// <summary>
    /// Called when Initialize message is received.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        Debug.Log($"[{gameObject.name}] Initialized!");
    }

    /// <summary>
    /// Called when Refresh message is received.
    /// </summary>
    public override void Refresh(List<MmTransform> transformList)
    {
        Debug.Log($"[{gameObject.name}] Refreshed");
    }
}
