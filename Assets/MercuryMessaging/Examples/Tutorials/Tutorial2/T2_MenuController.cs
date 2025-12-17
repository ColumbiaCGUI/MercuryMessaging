using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Tutorial 2: Menu system example demonstrating child-to-parent notification.
/// Shows how child buttons can notify parent of clicks using ToParents().
///
/// Hierarchy Setup:
/// T2_MenuManager (MmRelayNode + T2_MenuController)
///   ├── MainMenu (MmRelayNode + T2_ButtonResponder)
///   │     ├── PlayButton (T2_ButtonResponder)
///   │     └── SettingsButton (T2_ButtonResponder)
///   └── SettingsMenu (MmRelayNode + T2_ButtonResponder)
///         └── BackButton (T2_ButtonResponder)
/// </summary>
public class T2_MenuController : MmBaseResponder
{
    private MmRelayNode relay;

    new void Start()
    {
        relay = GetComponent<MmRelayNode>();
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        // Deactivate all menus first
        relay.Send(false).ToChildren().Execute();
        // Then activate MainMenu specifically (by tag)
        relay.Send(true).ToChildren().WithTag(MmTag.Tag0).Execute();
        Debug.Log("[MenuController] Showing MainMenu");
    }

    public void ShowSettings()
    {
        relay.Send(false).ToChildren().Execute();
        relay.Send(true).ToChildren().WithTag(MmTag.Tag1).Execute();
        Debug.Log("[MenuController] Showing SettingsMenu");
    }

    // Child buttons notify parent when clicked
    protected override void ReceivedMessage(MmMessageString msg)
    {
        switch (msg.value)
        {
            case "PlayClicked":
                StartGame();
                break;
            case "SettingsClicked":
                ShowSettings();
                break;
            case "BackClicked":
                ShowMainMenu();
                break;
            default:
                Debug.Log($"[MenuController] Received: {msg.value}");
                break;
        }
    }

    void StartGame()
    {
        Debug.Log("[MenuController] Starting game...");
    }
}
