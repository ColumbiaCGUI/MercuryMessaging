using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;

/// <summary>
/// Tutorial 10: Gameplay state responder.
/// Handles gameplay-specific setup, cursor locking, and time scale.
///
/// Keyboard Controls:
/// Escape - Pause game
/// </summary>
public class T10_GameplayBehavior : MmAppStateResponder
{
    [Header("UI References (Optional)")]
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject player;

    [Header("Gameplay Settings")]
    [SerializeField] private bool lockCursor = true;
    [SerializeField] private bool hideCursor = true;

    // Gameplay state
    private int score;
    private float playTime;
    private bool isPlaying;

    // Reference to app controller
    private T10_MyAppController appController;

    public override void Initialize()
    {
        base.Initialize();

        appController = GetComponentInParent<T10_MyAppController>();
        Debug.Log("[T10_Gameplay] Initialized");
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);

        Debug.Log($"[T10_Gameplay] SetActive({active})");

        // Show/hide gameplay elements
        if (gameplayUI != null)
            gameplayUI.SetActive(active);

        if (player != null)
            player.SetActive(active);

        if (active)
        {
            OnStateEnter();
        }
        else
        {
            OnStateExit();
        }
    }

    void OnStateEnter()
    {
        // Gameplay-specific setup
        if (hideCursor)
            Cursor.visible = false;

        if (lockCursor)
            Cursor.lockState = CursorLockMode.Locked;

        Time.timeScale = 1f;
        isPlaying = true;

        StateText = "Playing";

        Debug.Log("[T10_Gameplay] Gameplay started - Press Escape to pause");
    }

    void OnStateExit()
    {
        isPlaying = false;

        // Unlock cursor when leaving gameplay
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    new void Update()
    {
        if (!isPlaying) return;

        // Track play time
        playTime += Time.deltaTime;

        // Update state text with play info
        StateText = $"Score: {score} | Time: {playTime:F0}s";

        // Example: Press P to add score
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddScore(10);
        }
    }

    /// <summary>
    /// Add points to the score.
    /// </summary>
    public void AddScore(int points)
    {
        score += points;
        Debug.Log($"[T10_Gameplay] Score: {score}");
    }

    /// <summary>
    /// Get the current score.
    /// </summary>
    public int Score => score;

    /// <summary>
    /// Get the total play time.
    /// </summary>
    public float PlayTime => playTime;

    /// <summary>
    /// Reset gameplay state.
    /// </summary>
    public void ResetGameplay()
    {
        score = 0;
        playTime = 0f;
        Debug.Log("[T10_Gameplay] Gameplay reset");
    }

    /// <summary>
    /// Pause the game (notify parent to switch to Pause state).
    /// </summary>
    public void PauseGame()
    {
        if (appController != null)
        {
            appController.GoToPause();
        }
    }

    /// <summary>
    /// End the game and return to menu.
    /// </summary>
    public void EndGame()
    {
        if (appController != null)
        {
            Debug.Log($"[T10_Gameplay] Game ended - Final Score: {score}");
            appController.GoToMainMenu();
        }
    }

    public override void Refresh(List<MmTransform> transformList)
    {
        Debug.Log("[T10_Gameplay] Refreshed");
    }

    /// <summary>
    /// Handle incoming integer messages (e.g., score updates).
    /// </summary>
    protected override void ReceivedMessage(MmMessageInt msg)
    {
        AddScore(msg.value);
    }
}
