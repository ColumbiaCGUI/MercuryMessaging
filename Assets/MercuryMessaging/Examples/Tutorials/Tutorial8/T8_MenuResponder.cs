using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;

/// <summary>
/// Tutorial 8: Main menu state responder.
/// Active when FSM is in "MainMenu" state.
/// </summary>
public class T8_MenuResponder : MmBaseResponder
{
    [Header("UI Reference (Optional)")]
    [SerializeField] private GameObject menuCanvas;

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("[MainMenu] Initialized - Press Enter to start game");

        if (menuCanvas != null)
            menuCanvas.SetActive(true);
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);
        Debug.Log($"[MainMenu] SetActive({active})");

        if (menuCanvas != null)
            menuCanvas.SetActive(active);
    }

    public override void Refresh(List<MmTransform> transformList)
    {
        Debug.Log("[MainMenu] Refreshed");
    }

    protected override void ReceivedMessage(MmMessageString msg)
    {
        Debug.Log($"[MainMenu] Received: {msg.value}");
    }
}
