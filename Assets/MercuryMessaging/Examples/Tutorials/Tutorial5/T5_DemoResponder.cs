using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;

/// <summary>
/// Tutorial 5: Demo responder that logs all received messages.
/// Used by T5_DSLSceneSetup to demonstrate DSL message routing.
/// </summary>
public class T5_DemoResponder : MmBaseResponder
{
    protected override void ReceivedMessage(MmMessageString msg)
    {
        Debug.Log($"  [{gameObject.name}] Received string: \"{msg.value}\"");
    }

    protected override void ReceivedMessage(MmMessageInt msg)
    {
        Debug.Log($"  [{gameObject.name}] Received int: {msg.value}");
    }

    protected override void ReceivedMessage(MmMessageFloat msg)
    {
        Debug.Log($"  [{gameObject.name}] Received float: {msg.value}");
    }

    protected override void ReceivedMessage(MmMessageBool msg)
    {
        Debug.Log($"  [{gameObject.name}] Received bool: {msg.value}");
    }

    protected override void ReceivedMessage(MmMessageVector3 msg)
    {
        Debug.Log($"  [{gameObject.name}] Received Vector3: {msg.value}");
    }

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log($"  [{gameObject.name}] Initialize() called");
    }

    public override void Refresh(List<MmTransform> transformList)
    {
        Debug.Log($"  [{gameObject.name}] Refresh() called");
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);
        Debug.Log($"  [{gameObject.name}] SetActive({active}) called");
    }
}
