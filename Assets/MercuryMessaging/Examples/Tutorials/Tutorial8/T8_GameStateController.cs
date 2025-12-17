using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Tutorial 8: Game state controller using MmRelaySwitchNode FSM.
/// Demonstrates state machine navigation and Selected filter.
///
/// Hierarchy Setup (all children need MmRelayNode):
/// T8_GameManager (MmRelaySwitchNode + T8_GameStateController)
///   ├── MainMenu (MmRelayNode + T8_MenuResponder)
///   ├── Gameplay (MmRelayNode + T8_GameplayResponder)
///   ├── PauseMenu (MmRelayNode + T8_PauseResponder)
///   └── GameOver (MmRelayNode + T8_GameOverResponder)
///
/// Keyboard Controls:
/// Return/Enter - Start game (from MainMenu) / Restart (from GameOver)
/// Escape - Pause (from Gameplay) / Resume (from Pause)
/// Q - Quit to menu (from Gameplay or Pause)
/// G - Trigger game over (from Gameplay)
/// 1-4 - Jump directly to state
/// </summary>
public class T8_GameStateController : MonoBehaviour
{
    private MmRelaySwitchNode switchNode;
    private FiniteStateMachine<MmRoutingTableItem> fsm;

    // State names (must match child GameObject names)
    public const string STATE_MAIN_MENU = "MainMenu";
    public const string STATE_GAMEPLAY = "Gameplay";
    public const string STATE_PAUSE = "PauseMenu";
    public const string STATE_GAME_OVER = "GameOver";

    void Start()
    {
        switchNode = GetComponent<MmRelaySwitchNode>();

        if (switchNode == null)
        {
            Debug.LogError("[T8_GameStateController] Missing MmRelaySwitchNode component!");
            return;
        }

        fsm = switchNode.RespondersFSM;

        // Setup state change callbacks
        SetupStateCallbacks();

        // Start at main menu
        GoToMainMenu();

        Debug.Log("[T8_GameStateController] Game started. Press Enter to start, Escape to pause.");
    }

    void SetupStateCallbacks()
    {
        // Global events (called on every state change)
        fsm.GlobalEnter += OnAnyStateEnter;
        fsm.GlobalExit += OnAnyStateExit;

        // Per-state events (find states by name)
        foreach (var item in switchNode.RoutingTable)
        {
            if (item.Name == STATE_GAMEPLAY)
            {
                fsm[item].Enter += OnGameplayEnter;
                fsm[item].Exit += OnGameplayExit;
            }
            else if (item.Name == STATE_PAUSE)
            {
                fsm[item].Enter += OnPauseEnter;
                fsm[item].Exit += OnPauseExit;
            }
        }
    }

    #region State Callbacks

    void OnAnyStateEnter()
    {
        Debug.Log($"[FSM] Entered: {switchNode.CurrentName}");

        // Initialize the new state's responders
        switchNode.Send(MmMethod.Initialize)
            .ToChildren()
            .Selected()
            .Execute();
    }

    void OnAnyStateExit()
    {
        Debug.Log($"[FSM] Exited: {fsm.Previous?.Name ?? "none"}");
    }

    void OnGameplayEnter()
    {
        Debug.Log("[FSM] Gameplay started - Time resumed");
        Time.timeScale = 1f;
    }

    void OnGameplayExit()
    {
        Debug.Log("[FSM] Leaving gameplay");
    }

    void OnPauseEnter()
    {
        Debug.Log("[FSM] Game paused - Time frozen");
        Time.timeScale = 0f;
    }

    void OnPauseExit()
    {
        Debug.Log("[FSM] Unpausing - Time resumed");
        Time.timeScale = 1f;
    }

    #endregion

    #region Public State Transitions

    public void GoToMainMenu() => JumpToState(STATE_MAIN_MENU);
    public void StartGame() => JumpToState(STATE_GAMEPLAY);
    public void PauseGame() => JumpToState(STATE_PAUSE);
    public void ResumeGame() => JumpToState(STATE_GAMEPLAY);
    public void GameOver() => JumpToState(STATE_GAME_OVER);

    #endregion

    #region State Query Methods

    public bool IsInState(string stateName) => switchNode.CurrentName == stateName;
    public bool IsPlaying => IsInState(STATE_GAMEPLAY);
    public bool IsPaused => IsInState(STATE_PAUSE);
    public bool IsInMenu => IsInState(STATE_MAIN_MENU);
    public bool IsGameOver => IsInState(STATE_GAME_OVER);

    public string CurrentStateName => switchNode.CurrentName;

    #endregion

    void JumpToState(string stateName)
    {
        foreach (var item in switchNode.RoutingTable)
        {
            if (item.Name == stateName)
            {
                fsm.JumpTo(item);
                return;
            }
        }
        Debug.LogWarning($"[T8_GameStateController] State not found: {stateName}");
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Enter - Start game or restart
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (IsInMenu) StartGame();
            else if (IsGameOver) GoToMainMenu();
        }

        // Escape - Toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPlaying) PauseGame();
            else if (IsPaused) ResumeGame();
        }

        // Q - Quit to menu
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (IsPlaying || IsPaused)
            {
                Time.timeScale = 1f; // Ensure time is restored
                GoToMainMenu();
            }
        }

        // G - Trigger game over (from gameplay)
        if (Input.GetKeyDown(KeyCode.G) && IsPlaying)
        {
            GameOver();
        }

        // 1-4 - Direct state jump (for debugging)
        if (Input.GetKeyDown(KeyCode.Alpha1)) GoToMainMenu();
        if (Input.GetKeyDown(KeyCode.Alpha2)) StartGame();
        if (Input.GetKeyDown(KeyCode.Alpha3)) PauseGame();
        if (Input.GetKeyDown(KeyCode.Alpha4)) GameOver();
    }

    void OnDestroy()
    {
        // Ensure time is restored when destroyed
        Time.timeScale = 1f;
    }
}
