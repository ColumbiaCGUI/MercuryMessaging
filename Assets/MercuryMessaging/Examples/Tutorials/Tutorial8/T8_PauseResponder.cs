using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;

/// <summary>
/// Tutorial 8: Pause menu state responder.
/// Active when FSM is in "PauseMenu" state.
/// </summary>
public class T8_PauseResponder : MmBaseResponder
{
    [Header("UI Reference (Optional)")]
    [SerializeField] private GameObject pauseCanvas;

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("[Pause] Initialized - Game paused");
        Debug.Log("[Pause] Press Escape to resume, Q to quit");

        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);
        Debug.Log($"[Pause] SetActive({active})");

        if (pauseCanvas != null)
            pauseCanvas.SetActive(active);
    }

    public override void Refresh(List<MmTransform> transformList)
    {
        Debug.Log("[Pause] Refreshed");
    }

    protected override void ReceivedMessage(MmMessageString msg)
    {
        Debug.Log($"[Pause] Received: {msg.value}");
    }
}
