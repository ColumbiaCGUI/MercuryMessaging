using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Tutorial 3: Traditional approach using MmInvoke override.
/// Demonstrates handling custom methods with switch statement.
///
/// This is the original way - see T3_EnemyResponderExtendable.cs for
/// the modern, cleaner approach with MmExtendableResponder.
/// </summary>
public class T3_EnemyResponder : MmBaseResponder
{
    [SerializeField] private int health = 100;
    [SerializeField] private Renderer meshRenderer;

    /// <summary>
    /// Override MmInvoke to handle custom methods.
    /// IMPORTANT: Always call base.MmInvoke() first!
    /// </summary>
    public override void MmInvoke(MmMessage message)
    {
        // CRITICAL: Always call base first for standard methods!
        base.MmInvoke(message);

        // Handle custom methods via switch
        switch ((int)message.MmMethod)
        {
            case T3_MyMethods.TakeDamage:
                HandleDamage((MmMessageInt)message);
                break;

            case T3_MyMethods.ChangeColor:
                HandleColorChange((MmMessageVector3)message);
                break;

            case T3_MyMethods.EnableGravity:
                HandleGravity((MmMessageBool)message);
                break;

            case T3_MyMethods.Heal:
                HandleHeal((MmMessageInt)message);
                break;
        }
    }

    private void HandleDamage(MmMessageInt msg)
    {
        health -= msg.value;
        Debug.Log($"[{gameObject.name}] Took {msg.value} damage. Health: {health}");

        if (health <= 0)
        {
            Debug.Log($"[{gameObject.name}] Died!");
            // Notify parent of death
            var relay = GetComponent<MmRelayNode>();
            if (relay != null)
            {
                relay.Send("EnemyDied").ToParents().Execute();
            }
            Destroy(gameObject);
        }
    }

    private void HandleColorChange(MmMessageVector3 msg)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = new Color(msg.value.x, msg.value.y, msg.value.z);
            Debug.Log($"[{gameObject.name}] Color changed to ({msg.value.x}, {msg.value.y}, {msg.value.z})");
        }
    }

    private void HandleGravity(MmMessageBool msg)
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = msg.value;
            Debug.Log($"[{gameObject.name}] Gravity {(msg.value ? "enabled" : "disabled")}");
        }
    }

    private void HandleHeal(MmMessageInt msg)
    {
        health += msg.value;
        Debug.Log($"[{gameObject.name}] Healed {msg.value}. Health: {health}");
    }

    // Standard methods still work via base class
    public override void Initialize()
    {
        base.Initialize();
        Debug.Log($"[{gameObject.name}] Initialized with {health} health");
    }
}
