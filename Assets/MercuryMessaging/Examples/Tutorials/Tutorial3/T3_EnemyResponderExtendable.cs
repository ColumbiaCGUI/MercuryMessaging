using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Tutorial 3: Modern approach using MmExtendableResponder.
/// Demonstrates RegisterCustomHandler pattern - 50% less code!
///
/// Benefits over traditional approach:
/// - No switch statements
/// - Can't forget base.MmInvoke() (automatic)
/// - Dynamic handler registration/unregistration at runtime
/// - Same performance (fast path less than 200ns, slow path less than 500ns)
/// </summary>
public class T3_EnemyResponderExtendable : MmExtendableResponder
{
    [SerializeField] private int health = 100;
    [SerializeField] private Renderer meshRenderer;

    public override void Awake()
    {
        base.Awake();

        // Register custom handlers - clean and explicit!
        RegisterCustomHandler((MmMethod)T3_MyMethods.TakeDamage, OnTakeDamage);
        RegisterCustomHandler((MmMethod)T3_MyMethods.ChangeColor, OnChangeColor);
        RegisterCustomHandler((MmMethod)T3_MyMethods.EnableGravity, OnEnableGravity);
        RegisterCustomHandler((MmMethod)T3_MyMethods.Heal, OnHeal);
    }

    private void OnTakeDamage(MmMessage message)
    {
        var msg = (MmMessageInt)message;
        health -= msg.value;
        Debug.Log($"[{gameObject.name}] Took {msg.value} damage. Health: {health}");

        if (health <= 0)
        {
            Debug.Log($"[{gameObject.name}] Died!");
            this.Send("EnemyDied").ToParents().Execute();
            Destroy(gameObject);
        }
    }

    private void OnChangeColor(MmMessage message)
    {
        var msg = (MmMessageVector3)message;
        if (meshRenderer != null)
        {
            meshRenderer.material.color = new Color(msg.value.x, msg.value.y, msg.value.z);
            Debug.Log($"[{gameObject.name}] Color changed to ({msg.value.x}, {msg.value.y}, {msg.value.z})");
        }
    }

    private void OnEnableGravity(MmMessage message)
    {
        var msg = (MmMessageBool)message;
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = msg.value;
            Debug.Log($"[{gameObject.name}] Gravity {(msg.value ? "enabled" : "disabled")}");
        }
    }

    private void OnHeal(MmMessage message)
    {
        var msg = (MmMessageInt)message;
        health += msg.value;
        Debug.Log($"[{gameObject.name}] Healed {msg.value}. Health: {health}");
    }

    // Standard methods still work normally
    public override void Initialize()
    {
        base.Initialize();
        Debug.Log($"[{gameObject.name}] Initialized with {health} health");
    }
}
