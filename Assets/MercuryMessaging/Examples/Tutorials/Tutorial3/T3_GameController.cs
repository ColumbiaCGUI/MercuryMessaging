using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Tutorial 3: Game controller demonstrating sending custom methods.
///
/// Hierarchy Setup:
/// T3_GameWorld (MmRelayNode + T3_GameController)
///   ├── Player (MmRelayNode + PlayerResponder)
///   └── Enemies (MmRelayNode)
///         ├── Enemy1 (MmRelayNode + T3_EnemyResponderExtendable)
///         ├── Enemy2 (MmRelayNode + T3_EnemyResponderExtendable)
///         └── Enemy3 (MmRelayNode + T3_EnemyResponderExtendable)
///
/// Keyboard Controls:
/// D - Damage all enemies (10 damage)
/// C - Change enemy colors to red
/// G - Toggle gravity on enemies
/// H - Heal all enemies (25 health)
/// I - Initialize all enemies
/// </summary>
public class T3_GameController : MonoBehaviour
{
    private MmRelayNode relay;
    private bool gravityEnabled = true;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();

        // Initialize all descendants
        relay.BroadcastInitialize();
    }

    void Update()
    {
        // D - Damage all enemies
        if (Input.GetKeyDown(KeyCode.D))
        {
            DamageAllEnemies(10);
        }

        // C - Change enemy colors to red
        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeEnemyColors(Color.red);
        }

        // G - Toggle gravity
        if (Input.GetKeyDown(KeyCode.G))
        {
            gravityEnabled = !gravityEnabled;
            ToggleGravity(gravityEnabled);
        }

        // H - Heal all enemies
        if (Input.GetKeyDown(KeyCode.H))
        {
            HealAllEnemies(25);
        }

        // I - Re-initialize
        if (Input.GetKeyDown(KeyCode.I))
        {
            relay.BroadcastInitialize();
            Debug.Log("[GameController] Re-initialized all enemies");
        }
    }

    public void DamageAllEnemies(int damage)
    {
        // Using DSL with custom method
        relay.Send((MmMethod)T3_MyMethods.TakeDamage, damage)
            .ToDescendants()
            .Execute();
        Debug.Log($"[GameController] Dealt {damage} damage to all enemies");
    }

    public void ChangeEnemyColors(Color color)
    {
        // Custom method with Vector3 payload (Color as RGB)
        relay.Send((MmMethod)T3_MyMethods.ChangeColor,
            new Vector3(color.r, color.g, color.b))
            .ToDescendants()
            .Execute();
        Debug.Log($"[GameController] Changed enemy colors to {color}");
    }

    public void ToggleGravity(bool enabled)
    {
        relay.Send((MmMethod)T3_MyMethods.EnableGravity, enabled)
            .ToDescendants()
            .Execute();
        Debug.Log($"[GameController] Gravity {(enabled ? "enabled" : "disabled")}");
    }

    public void HealAllEnemies(int amount)
    {
        relay.Send((MmMethod)T3_MyMethods.Heal, amount)
            .ToDescendants()
            .Execute();
        Debug.Log($"[GameController] Healed all enemies for {amount}");
    }

    // Receive notifications from children (e.g., enemy death)
    private void OnEnable()
    {
        // Listen for enemy death notifications
        var responder = GetComponent<MmBaseResponder>();
        if (responder == null)
        {
            // Add a simple responder to receive messages
            responder = gameObject.AddComponent<T3_GameControllerResponder>();
        }
    }
}

/// <summary>
/// Simple responder to receive notifications from children (like enemy death).
/// </summary>
public class T3_GameControllerResponder : MmBaseResponder
{
    protected override void ReceivedMessage(MmMessageString msg)
    {
        if (msg.value == "EnemyDied")
        {
            Debug.Log("[GameController] An enemy has been destroyed!");
        }
    }
}
