using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;

/// <summary>
/// Tutorial 8: Gameplay state responder.
/// Active when FSM is in "Gameplay" state.
/// </summary>
public class T8_GameplayResponder : MmBaseResponder
{
    [Header("Game State")]
    [SerializeField] private int score = 0;
    [SerializeField] private float gameTime = 0f;

    private bool isActive = false;

    public override void Initialize()
    {
        base.Initialize();
        score = 0;
        gameTime = 0f;
        isActive = true;
        Debug.Log("[Gameplay] Initialized - Game started!");
        Debug.Log("[Gameplay] Press G for game over, Escape to pause");
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);
        isActive = active;
        Debug.Log($"[Gameplay] SetActive({active})");
    }

    public override void Refresh(List<MmTransform> transformList)
    {
        Debug.Log($"[Gameplay] Refreshed - Score: {score}, Time: {gameTime:F1}s");
    }

    protected override void ReceivedMessage(MmMessageString msg)
    {
        switch (msg.value)
        {
            case "AddScore":
                score += 10;
                Debug.Log($"[Gameplay] Score: {score}");
                break;
            case "LoseLife":
                Debug.Log("[Gameplay] Lost a life!");
                break;
            default:
                Debug.Log($"[Gameplay] Received: {msg.value}");
                break;
        }
    }

    protected override void ReceivedMessage(MmMessageInt msg)
    {
        score += msg.value;
        Debug.Log($"[Gameplay] Score +{msg.value} = {score}");
    }

    new void Update()
    {
        if (isActive)
        {
            gameTime += Time.deltaTime;
        }
    }

    public int GetScore() => score;
    public float GetGameTime() => gameTime;
}
