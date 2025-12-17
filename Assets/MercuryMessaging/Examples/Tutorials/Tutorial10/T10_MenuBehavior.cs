using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;

/// <summary>
/// Tutorial 10: Main menu state responder.
/// Handles menu-specific setup, cursor visibility, and button interactions.
///
/// Keyboard Controls:
/// Space/Enter - Start game (go to Loading)
/// </summary>
public class T10_MenuBehavior : MmAppStateResponder
{
    [Header("UI References (Optional)")]
    [SerializeField] private GameObject menuCanvas;

    [Header("Menu Settings")]
    [SerializeField] private bool showCursor = true;

    // Reference to app controller for state navigation
    private T10_MyAppController appController;

    public override void Initialize()
    {
        base.Initialize();

        // Find the app controller in parent
        appController = GetComponentInParent<T10_MyAppController>();

        Debug.Log("[T10_Menu] Initialized");
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);

        Debug.Log($"[T10_Menu] SetActive({active})");

        // Show/hide menu UI
        if (menuCanvas != null)
            menuCanvas.SetActive(active);

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
        // Menu-specific setup
        Cursor.visible = showCursor;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;

        StateText = "Main Menu";

        Debug.Log("[T10_Menu] Press Space or Enter to start game");
    }

    void OnStateExit()
    {
        // Menu-specific cleanup
    }

    new void Update()
    {
        // Only process input when active
        if (!gameObject.activeInHierarchy) return;

        // Space or Enter to start game
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
        }
    }

    /// <summary>
    /// Start the game (go to Loading state).
    /// </summary>
    public void StartGame()
    {
        if (appController != null)
        {
            Debug.Log("[T10_Menu] Starting game...");
            appController.GoToLoading();
        }
    }

    /// <summary>
    /// Open options menu (could go to Options state).
    /// </summary>
    public void OpenOptions()
    {
        Debug.Log("[T10_Menu] Options not implemented in this tutorial");
    }

    /// <summary>
    /// Quit the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[T10_Menu] Quitting...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public override void Refresh(List<MmTransform> transformList)
    {
        Debug.Log("[T10_Menu] Refreshed");
    }
}
