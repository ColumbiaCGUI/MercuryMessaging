using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;

/// <summary>
/// Tutorial 10: Application state controller using MmAppStateSwitchResponder.
/// Manages application-level state transitions: Menu, Loading, Gameplay, Pause.
///
/// Hierarchy Setup:
/// AppController (MmRelaySwitchNode + T10_MyAppController)
///   ├── MainMenu (MmRelayNode + T10_MenuBehavior)
///   ├── Loading (MmRelayNode + T10_LoadingBehavior)
///   ├── Gameplay (MmRelayNode + T10_GameplayBehavior)
///   └── Pause (MmRelayNode + T10_PauseBehavior)
///
/// Keyboard Controls:
/// Escape - Toggle Pause (in Gameplay) / Resume (in Pause)
/// M - Go to Main Menu
/// </summary>
public class T10_MyAppController : MmAppStateSwitchResponder
{
    [Header("State Configuration")]
    [SerializeField] private string startState = "MainMenu";

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;

    // Track state history for "back" navigation
    private Stack<string> stateHistory = new Stack<string>();

    // Track time spent in each state
    private Dictionary<string, float> stateEnterTimes = new Dictionary<string, float>();

    // Events for external listeners
    public event System.Action<string> OnStateChanged;
    public event System.Action<string, float> OnStateExited;

    public override void Awake()
    {
        // Set initial state before base.Awake()
        InitialState = startState;
        base.Awake();
    }

    public override void Start()
    {
        base.Start();

        if (logStateChanges)
        {
            Debug.Log("[T10_AppController] Application state system initialized");
            Debug.Log("[T10_AppController] Press Escape to toggle pause, M for menu");
        }
    }

    /// <summary>
    /// Setup application states and register per-state callbacks.
    /// </summary>
    public override void SetupAppStates()
    {
        base.SetupAppStates();

        if (MmRelaySwitchNode == null || MmRelaySwitchNode.RespondersFSM == null)
        {
            Debug.LogWarning("[T10_AppController] MmRelaySwitchNode not ready");
            return;
        }

        var fsm = MmRelaySwitchNode.RespondersFSM;

        // Register enter/exit callbacks for each state
        foreach (var item in MmRelaySwitchNode.RoutingTable)
        {
            string stateName = item.Name;

            fsm[item].Enter += () =>
            {
                stateEnterTimes[stateName] = Time.time;

                if (logStateChanges)
                    Debug.Log($"[T10_AppController] Entered state: {stateName}");

                OnStateChanged?.Invoke(stateName);
                HandleStateEntered(stateName);
            };

            fsm[item].Exit += () =>
            {
                float duration = Time.time - stateEnterTimes.GetValueOrDefault(stateName, Time.time);

                if (logStateChanges)
                    Debug.Log($"[T10_AppController] Exited state: {stateName} (after {duration:F1}s)");

                OnStateExited?.Invoke(stateName, duration);
                HandleStateExited(stateName);
            };
        }

        Debug.Log($"[T10_AppController] Registered {MmRelaySwitchNode.RoutingTable.Count} app states");
    }

    new void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        string currentState = CurrentStateName;

        // Escape - Toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == "Gameplay")
            {
                GoToPause();
            }
            else if (currentState == "Pause")
            {
                GoToGameplay();
            }
        }

        // M - Go to main menu
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (currentState != "MainMenu")
            {
                GoToMainMenu();
            }
        }
    }

    #region Navigation Methods

    /// <summary>
    /// Get the current state name.
    /// </summary>
    public string CurrentStateName => MmRelaySwitchNode?.CurrentName ?? "";

    /// <summary>
    /// Navigate to Main Menu state.
    /// </summary>
    public void GoToMainMenu()
    {
        SwitchToState("MainMenu");
    }

    /// <summary>
    /// Navigate to Loading state.
    /// </summary>
    public void GoToLoading()
    {
        SwitchToState("Loading");
    }

    /// <summary>
    /// Navigate to Gameplay state.
    /// </summary>
    public void GoToGameplay()
    {
        SwitchToState("Gameplay");
    }

    /// <summary>
    /// Navigate to Pause state.
    /// </summary>
    public void GoToPause()
    {
        SwitchToState("Pause");
    }

    /// <summary>
    /// Navigate to a specific state by name with history tracking.
    /// </summary>
    public void GoTo(string stateName)
    {
        string current = CurrentStateName;
        if (!string.IsNullOrEmpty(current))
        {
            stateHistory.Push(current);
        }
        SwitchToState(stateName);
    }

    /// <summary>
    /// Go back to the previous state in history.
    /// </summary>
    public void GoBack()
    {
        if (stateHistory.Count > 0)
        {
            string previousState = stateHistory.Pop();
            SwitchToState(previousState);
        }
        else
        {
            if (logStateChanges)
                Debug.Log("[T10_AppController] No state history to go back to");
        }
    }

    private void SwitchToState(string stateName)
    {
        if (MmRelaySwitchNode == null) return;

        // Send Switch message to trigger FSM transition
        MmRelaySwitchNode.MmInvoke(
            MmMethod.Switch,
            stateName,
            new MmMetadataBlock(MmLevelFilter.Self, MmActiveFilter.All,
                default(MmSelectedFilter), MmNetworkFilter.Local)
        );
    }

    #endregion

    #region State Event Handlers

    /// <summary>
    /// Called when entering any state. Override for custom behavior.
    /// </summary>
    protected virtual void HandleStateEntered(string stateName)
    {
        switch (stateName)
        {
            case "Loading":
                // Start async loading process
                StartCoroutine(SimulateLoading());
                break;

            case "Gameplay":
                // Resume game systems
                break;

            case "Pause":
                // Pause game systems
                break;

            case "MainMenu":
                // Reset any game state if needed
                break;
        }
    }

    /// <summary>
    /// Called when exiting any state. Override for custom behavior.
    /// </summary>
    protected virtual void HandleStateExited(string stateName)
    {
        switch (stateName)
        {
            case "Gameplay":
                // Save game progress
                break;
        }
    }

    private System.Collections.IEnumerator SimulateLoading()
    {
        if (logStateChanges)
            Debug.Log("[T10_AppController] Loading started...");

        // Simulate async loading
        yield return new WaitForSeconds(2f);

        if (logStateChanges)
            Debug.Log("[T10_AppController] Loading complete!");

        // Transition to gameplay when done
        GoToGameplay();
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Check if currently in a specific state.
    /// </summary>
    public bool IsInState(string stateName)
    {
        return CurrentStateName == stateName;
    }

    /// <summary>
    /// Check if game is paused.
    /// </summary>
    public bool IsPaused => IsInState("Pause");

    /// <summary>
    /// Check if in gameplay.
    /// </summary>
    public bool IsPlaying => IsInState("Gameplay");

    /// <summary>
    /// Check if in main menu.
    /// </summary>
    public bool IsInMenu => IsInState("MainMenu");

    #endregion
}
