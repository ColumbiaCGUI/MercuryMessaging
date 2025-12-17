using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;

/// <summary>
/// Tutorial 10: Pause state responder.
/// Handles pause-specific setup, time scale freezing, and cursor visibility.
///
/// Keyboard Controls:
/// Escape - Resume game
/// M - Return to main menu
/// </summary>
public class T10_PauseBehavior : MmAppStateResponder
{
    [Header("UI References (Optional)")]
    [SerializeField] private GameObject pauseCanvas;

    [Header("Pause Settings")]
    [SerializeField] private bool freezeTime = true;
    [SerializeField] private bool showCursor = true;

    // Reference to app controller
    private T10_MyAppController appController;

    public override void Initialize()
    {
        base.Initialize();

        appController = GetComponentInParent<T10_MyAppController>();
        Debug.Log("[T10_Pause] Initialized");
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);

        Debug.Log($"[T10_Pause] SetActive({active})");

        // Show/hide pause UI
        if (pauseCanvas != null)
            pauseCanvas.SetActive(active);

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
        // Pause-specific setup
        if (freezeTime)
            Time.timeScale = 0f;

        if (showCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        StateText = "PAUSED";

        Debug.Log("[T10_Pause] Game paused - Press Escape to resume, M for menu");
    }

    void OnStateExit()
    {
        // Restore time when exiting pause
        Time.timeScale = 1f;
    }

    new void Update()
    {
        // Note: Update still runs when Time.timeScale = 0
        // because Update uses real time, not game time

        // These inputs are handled by the app controller,
        // but we could also handle them here:
        // if (Input.GetKeyDown(KeyCode.Escape)) ResumeGame();
        // if (Input.GetKeyDown(KeyCode.M)) ReturnToMenu();
    }

    /// <summary>
    /// Resume the game (go back to Gameplay state).
    /// </summary>
    public void ResumeGame()
    {
        if (appController != null)
        {
            Debug.Log("[T10_Pause] Resuming game...");
            appController.GoToGameplay();
        }
    }

    /// <summary>
    /// Return to main menu.
    /// </summary>
    public void ReturnToMenu()
    {
        if (appController != null)
        {
            Debug.Log("[T10_Pause] Returning to menu...");
            appController.GoToMainMenu();
        }
    }

    /// <summary>
    /// Quit the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[T10_Pause] Quitting...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public override void Refresh(List<MmTransform> transformList)
    {
        Debug.Log("[T10_Pause] Refreshed");
    }
}
