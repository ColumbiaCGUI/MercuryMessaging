// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// FSMConfigurationTutorial.cs - Tutorial for FSM Configuration DSL
// Part of DSL Overhaul Phase 11
//
// This tutorial demonstrates the fluent FSM configuration API:
// - ConfigureStates().OnEnter().OnExit().Build()
// - MmAppState.Configure().DefineState().Build()
// - Quick state navigation (GoTo, GoToPrevious, IsInState)

using System;
using UnityEngine;
using MercuryMessaging;


namespace MercuryMessaging.Examples.Tutorials.DSL
{
    /// <summary>
    /// Tutorial: FSM Configuration DSL
    ///
    /// MercuryMessaging supports state machines through MmRelaySwitchNode.
    /// The DSL provides two complementary APIs:
    ///
    /// 1. ConfigureStates() - Add callbacks to existing FSM states
    /// 2. MmAppState.Configure() - Full state machine definition
    ///
    /// Both provide fluent, type-safe configuration.
    /// </summary>
    public class FSMConfigurationTutorial : MonoBehaviour
    {
        private MmRelaySwitchNode switchNode;

        void Start()
        {
            switchNode = GetComponent<MmRelaySwitchNode>();
            if (switchNode == null)
            {
                Debug.LogError("FSMConfigurationTutorial requires an MmRelaySwitchNode component");
                return;
            }

            // Choose one approach:
            // ConfigureStatesExample();  // Simpler: Just add callbacks
            AppStateExample();            // Full: Define complete state machine
        }

        #region ConfigureStates API (Phase 2)

        /// <summary>
        /// ConfigureStates() adds callbacks to an existing FSM.
        /// Use when states are already defined in the hierarchy.
        /// </summary>
        void ConfigureStatesExample()
        {
            Debug.Log("=== CONFIGURE STATES API ===");

            // =========================================
            // BASIC CALLBACK REGISTRATION
            // =========================================

            switchNode.ConfigureStates()
                // Global callback - called for ALL state changes
                .OnGlobalEnter(() =>
                {
                    Debug.Log($"Entered state: {switchNode.GetCurrentStateName()}");
                })

                // State-specific enter callback
                .OnEnter("MainMenu", () =>
                {
                    Debug.Log("Entered MainMenu - show UI");
                    // ShowMenuUI();
                })

                // State-specific exit callback
                .OnExit("MainMenu", () =>
                {
                    Debug.Log("Exiting MainMenu - hide UI");
                    // HideMenuUI();
                })

                // Multiple states in one chain
                .OnEnter("Gameplay", () => Debug.Log("Game started!"))
                .OnExit("Gameplay", () => Debug.Log("Game paused/ended"))
                .OnEnter("Pause", () => Debug.Log("Paused"))
                .OnEnter("GameOver", () => Debug.Log("Game Over!"))

                // Set initial state
                .StartWith("MainMenu")

                // Build and apply configuration
                .Build();

            // =========================================
            // QUICK CALLBACK REGISTRATION
            // =========================================

            // One-liner for simple callbacks
            switchNode.OnStateEnter("Settings", () => Debug.Log("Settings opened"));
            switchNode.OnStateExit("Settings", () => Debug.Log("Settings closed"));

            // =========================================
            // STATE NAVIGATION
            // =========================================

            // Go to specific state
            switchNode.GoTo("Gameplay");

            // Check current state
            if (switchNode.IsInState("Gameplay"))
            {
                Debug.Log("Currently playing!");
            }

            // Get current state name
            string currentState = switchNode.GetCurrentStateName();
            Debug.Log($"Current state: {currentState}");

            // Go back to previous state
            if (switchNode.GoToPrevious())
            {
                Debug.Log($"Returned to: {switchNode.GetCurrentStateName()}");
            }

            Debug.Log("ConfigureStates example complete");
        }

        #endregion

        #region MmAppState API (Phase 8)

        /// <summary>
        /// MmAppState.Configure() provides full state machine definition.
        /// Use for complete control over state behavior.
        /// </summary>
        void AppStateExample()
        {
            Debug.Log("=== APP STATE API ===");

            // =========================================
            // FULL STATE MACHINE DEFINITION
            // =========================================

            MmAppState.Configure(switchNode)
                // Define MainMenu state
                .DefineState("MainMenu")
                    .OnEnter(() =>
                    {
                        Debug.Log("==> MainMenu: Show menu UI");
                        // ShowMenuCanvas();
                        // EnableMenuMusic();
                    })
                    .OnExit(() =>
                    {
                        Debug.Log("<== MainMenu: Hide menu UI");
                        // HideMenuCanvas();
                    })

                // Define Gameplay state
                .DefineState("Gameplay")
                    .OnEnter(() =>
                    {
                        Debug.Log("==> Gameplay: Start game");
                        // EnablePlayerControls();
                        // StartGameMusic();
                        // ResetScore();
                    })
                    .OnExit(() =>
                    {
                        Debug.Log("<== Gameplay: Stop game");
                        // DisablePlayerControls();
                    })

                // Define Pause state
                .DefineState("Pause")
                    .OnTransition(
                        onEnter: () =>
                        {
                            Debug.Log("==> Pause: Game paused");
                            Time.timeScale = 0;
                            // ShowPauseMenu();
                        },
                        onExit: () =>
                        {
                            Debug.Log("<== Pause: Game resumed");
                            Time.timeScale = 1;
                            // HidePauseMenu();
                        }
                    )

                // Define GameOver state
                .DefineState("GameOver")
                    .OnEnter(() =>
                    {
                        Debug.Log("==> GameOver: Show results");
                        // ShowGameOverScreen();
                        // SaveHighScore();
                    })

                // Global callback for any state change
                .OnAnyStateEnter(() =>
                {
                    Debug.Log($"[Global] Now in state: {switchNode.GetCurrentStateName()}");
                })

                // Set initial state
                .StartWith("MainMenu")

                // Build and apply
                .Build();

            // =========================================
            // STATE NAVIGATION
            // =========================================

            // Navigate between states
            Debug.Log("\n--- State Navigation Demo ---");

            // Start game
            switchNode.GoTo("Gameplay");

            // Pause
            switchNode.GoTo("Pause");

            // Resume (go back)
            switchNode.GoToPrevious();

            // Game over
            switchNode.GoTo("GameOver");

            // Return to menu
            switchNode.GoTo("MainMenu");

            Debug.Log("AppState example complete");
        }

        #endregion

        #region Practical Example: Game Menu System

        /// <summary>
        /// Real-world example: Complete game menu system.
        /// </summary>
        public void SetupGameMenuSystem()
        {
            // Reference to UI canvases (would be assigned in Inspector)
            // [SerializeField] private Canvas mainMenuCanvas;
            // [SerializeField] private Canvas gameplayHUD;
            // [SerializeField] private Canvas pauseMenu;
            // [SerializeField] private Canvas settingsMenu;
            // [SerializeField] private Canvas gameOverScreen;

            MmAppState.Configure(switchNode)
                .DefineState("MainMenu")
                    .OnEnter(() =>
                    {
                        // mainMenuCanvas.gameObject.SetActive(true);
                        // AudioManager.PlayMenuMusic();
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    })
                    .OnExit(() =>
                    {
                        // mainMenuCanvas.gameObject.SetActive(false);
                    })

                .DefineState("Settings")
                    .OnEnter(() =>
                    {
                        // settingsMenu.gameObject.SetActive(true);
                        // LoadSettings();
                    })
                    .OnExit(() =>
                    {
                        // settingsMenu.gameObject.SetActive(false);
                        // SaveSettings();
                    })

                .DefineState("Loading")
                    .OnEnter(() =>
                    {
                        // ShowLoadingScreen();
                        // StartCoroutine(LoadGameAsync());
                    })

                .DefineState("Gameplay")
                    .OnEnter(() =>
                    {
                        // gameplayHUD.gameObject.SetActive(true);
                        Time.timeScale = 1;
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                        // EnablePlayerInput();
                    })
                    .OnExit(() =>
                    {
                        // gameplayHUD.gameObject.SetActive(false);
                        // DisablePlayerInput();
                    })

                .DefineState("Pause")
                    .OnEnter(() =>
                    {
                        // pauseMenu.gameObject.SetActive(true);
                        Time.timeScale = 0;
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    })
                    .OnExit(() =>
                    {
                        // pauseMenu.gameObject.SetActive(false);
                        Time.timeScale = 1;
                    })

                .DefineState("GameOver")
                    .OnEnter(() =>
                    {
                        // gameOverScreen.gameObject.SetActive(true);
                        // DisplayFinalScore();
                        // CheckHighScore();
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    })
                    .OnExit(() =>
                    {
                        // gameOverScreen.gameObject.SetActive(false);
                    })

                .OnAnyStateEnter(() =>
                {
                    Debug.Log($"Game state changed to: {switchNode.GetCurrentStateName()}");
                    // Analytics.LogStateChange(switchNode.GetCurrentStateName());
                })

                .StartWith("MainMenu")
                .Build();
        }

        // Public methods for UI buttons
        public void OnStartGame() => switchNode.GoTo("Loading");
        public void OnOpenSettings() => switchNode.GoTo("Settings");
        public void OnPauseGame() => switchNode.GoTo("Pause");
        public void OnResumeGame() => switchNode.GoToPrevious();
        public void OnQuitToMenu() => switchNode.GoTo("MainMenu");
        public void OnRetry() => switchNode.GoTo("Gameplay");

        #endregion
    }

    /// <summary>
    /// Minimal example showing just the essentials.
    /// </summary>
    public class MinimalFSMExample : MonoBehaviour
    {
        void Start()
        {
            var switchNode = GetComponent<MmRelaySwitchNode>();

            // Simplest possible FSM configuration
            switchNode.ConfigureStates()
                .OnEnter("StateA", () => Debug.Log("A"))
                .OnEnter("StateB", () => Debug.Log("B"))
                .StartWith("StateA")
                .Build();

            // Navigate
            switchNode.GoTo("StateB");        // Prints "B"
            switchNode.GoTo("StateA");        // Prints "A"
            switchNode.GoToPrevious();        // Back to B, prints "B"

            // Query
            bool inA = switchNode.IsInState("StateA");  // false
            string current = switchNode.GetCurrentStateName();  // "StateB"
        }
    }
}
