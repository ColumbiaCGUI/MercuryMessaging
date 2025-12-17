using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;

/// <summary>
/// Tutorial 10: Loading state responder.
/// Displays loading progress and auto-advances when complete.
/// </summary>
public class T10_LoadingBehavior : MmAppStateResponder
{
    [Header("UI References (Optional)")]
    [SerializeField] private GameObject loadingCanvas;

    [Header("Loading Settings")]
    [SerializeField] private float simulatedLoadTime = 2f;
    [SerializeField] private bool autoAdvance = true;

    // Loading state
    private float loadProgress;
    private bool isLoading;

    // Reference to app controller
    private T10_MyAppController appController;

    public override void Initialize()
    {
        base.Initialize();

        appController = GetComponentInParent<T10_MyAppController>();
        Debug.Log("[T10_Loading] Initialized");
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);

        Debug.Log($"[T10_Loading] SetActive({active})");

        // Show/hide loading UI
        if (loadingCanvas != null)
            loadingCanvas.SetActive(active);

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
        // Reset loading state
        loadProgress = 0f;
        isLoading = true;

        // Update state text
        StateText = "Loading...";

        Debug.Log("[T10_Loading] Loading started");

        if (autoAdvance)
        {
            StartCoroutine(SimulateLoading());
        }
    }

    void OnStateExit()
    {
        isLoading = false;
        StopAllCoroutines();
    }

    private System.Collections.IEnumerator SimulateLoading()
    {
        float elapsed = 0f;

        while (elapsed < simulatedLoadTime)
        {
            elapsed += Time.deltaTime;
            loadProgress = Mathf.Clamp01(elapsed / simulatedLoadTime);

            // Update state text with progress
            StateText = $"Loading... {loadProgress * 100:F0}%";

            yield return null;
        }

        // Loading complete
        loadProgress = 1f;
        isLoading = false;
        StateText = "Loading Complete!";

        Debug.Log("[T10_Loading] Loading complete!");

        // Auto-advance to gameplay
        yield return new WaitForSeconds(0.5f);

        if (appController != null)
        {
            appController.GoToGameplay();
        }
    }

    /// <summary>
    /// Update loading progress externally (for real asset loading).
    /// </summary>
    public void UpdateProgress(float progress)
    {
        loadProgress = Mathf.Clamp01(progress);
        StateText = $"Loading... {loadProgress * 100:F0}%";
    }

    /// <summary>
    /// Mark loading as complete (for real asset loading).
    /// </summary>
    public void CompleteLoading()
    {
        loadProgress = 1f;
        isLoading = false;
        StateText = "Loading Complete!";

        if (autoAdvance && appController != null)
        {
            appController.GoToGameplay();
        }
    }

    /// <summary>
    /// Get current loading progress (0-1).
    /// </summary>
    public float Progress => loadProgress;

    /// <summary>
    /// Check if currently loading.
    /// </summary>
    public bool IsLoading => isLoading;

    public override void Refresh(List<MmTransform> transformList)
    {
        Debug.Log("[T10_Loading] Refreshed");
    }
}
