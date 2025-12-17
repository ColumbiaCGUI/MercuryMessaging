using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;

/// <summary>
/// Tutorial 8: Game over state responder.
/// Active when FSM is in "GameOver" state.
/// </summary>
public class T8_GameOverResponder : MmBaseResponder
{
    [Header("UI Reference (Optional)")]
    [SerializeField] private GameObject gameOverCanvas;

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("[GameOver] Initialized - Game Over!");
        Debug.Log("[GameOver] Press Enter to return to menu");

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);
        Debug.Log($"[GameOver] SetActive({active})");

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(active);
    }

    public override void Refresh(List<MmTransform> transformList)
    {
        Debug.Log("[GameOver] Refreshed");
    }

    protected override void ReceivedMessage(MmMessageInt msg)
    {
        // Receive final score from gameplay
        Debug.Log($"[GameOver] Final score: {msg.value}");
    }

    protected override void ReceivedMessage(MmMessageString msg)
    {
        Debug.Log($"[GameOver] Received: {msg.value}");
    }
}
