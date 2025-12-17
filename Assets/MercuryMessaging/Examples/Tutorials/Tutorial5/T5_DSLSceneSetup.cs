using UnityEngine;
using MercuryMessaging;
using System.Collections;

/// <summary>
/// Tutorial 5: Interactive DSL API demonstration.
/// Creates a hierarchy and shows various DSL features.
///
/// Keyboard Controls:
/// 1 - Traditional vs Fluent comparison
/// 2 - Routing targets (Children/Descendants/Parents/All)
/// 3 - Typed values (int/float/string/bool/Vector3)
/// 4 - Combined filters (Active + Tag)
/// 5 - Auto-execute convenience methods
/// T - Delayed execution (2 second delay)
/// Y - Repeating messages (color cycling)
/// U - Conditional trigger (press SPACE to activate)
/// </summary>
public class T5_DSLSceneSetup : MonoBehaviour
{
    private MmRelayNode relay;
    private bool conditionalReady = false;
    private int colorIndex = 0;
    private Color[] cycleColors = { Color.red, Color.green, Color.blue, Color.yellow };

    [Header("Debug Options")]
    public bool logMessages = true;

    void Start()
    {
        // Setup hierarchy programmatically
        SetupHierarchy();
        relay = GetComponent<MmRelayNode>();

        Debug.Log("[DSL Demo] Press keys 1-5, T, Y, U to see different DSL features");
        Debug.Log("[DSL Demo] See console for output");
    }

    void SetupHierarchy()
    {
        // Ensure this object has a relay node
        if (GetComponent<MmRelayNode>() == null)
        {
            gameObject.AddComponent<MmRelayNode>();
        }

        // Create child hierarchy
        CreateChild("Child1", MmTag.Tag0);
        CreateChild("Child2", MmTag.Tag1);
        CreateChild("Child3", MmTag.Tag0);

        // Create nested hierarchy for descendant tests
        var nested = CreateChild("NestedParent", MmTag.Tag0);
        CreateNestedChild(nested, "Grandchild1");
        CreateNestedChild(nested, "Grandchild2");

        // Refresh routing tables
        GetComponent<MmRelayNode>().MmRefreshResponders();

        Debug.Log("[DSL Demo] Hierarchy created with 6 responders");
    }

    GameObject CreateChild(string name, MmTag tag)
    {
        var child = new GameObject(name);
        child.transform.SetParent(transform);

        var childRelay = child.AddComponent<MmRelayNode>();
        var responder = child.AddComponent<T5_DemoResponder>();
        responder.Tag = tag;
        responder.TagCheckEnabled = true;

        return child;
    }

    void CreateNestedChild(GameObject parent, string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent.transform);

        child.AddComponent<MmRelayNode>();
        var responder = child.AddComponent<T5_DemoResponder>();
    }

    void Update()
    {
        // 1 - Traditional vs Fluent comparison
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Demo1_TraditionalVsFluent();
        }

        // 2 - Routing targets
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Demo2_RoutingTargets();
        }

        // 3 - Typed values
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Demo3_TypedValues();
        }

        // 4 - Combined filters
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Demo4_CombinedFilters();
        }

        // 5 - Auto-execute methods
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Demo5_AutoExecute();
        }

        // T - Delayed execution
        if (Input.GetKeyDown(KeyCode.T))
        {
            DemoT_DelayedExecution();
        }

        // Y - Repeating messages
        if (Input.GetKeyDown(KeyCode.Y))
        {
            DemoY_RepeatingMessages();
        }

        // U - Set conditional ready
        if (Input.GetKeyDown(KeyCode.U))
        {
            DemoU_ConditionalTrigger();
        }

        // Space - Trigger conditional
        if (Input.GetKeyDown(KeyCode.Space) && conditionalReady)
        {
            conditionalReady = false;
            Debug.Log("[DSL Demo] Conditional triggered! Sending message...");
            relay.Send("Condition met!").ToDescendants().Execute();
        }
    }

    void Demo1_TraditionalVsFluent()
    {
        Debug.Log("=== Demo 1: Traditional vs Fluent ===");

        // Traditional way (verbose)
        Debug.Log("[Traditional] Sending with full metadata...");
        relay.MmInvoke(
            MmMethod.MessageString,
            "Traditional message",
            new MmMetadataBlock(
                MmLevelFilter.Child,
                MmActiveFilter.Active,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            )
        );

        // Fluent way (concise)
        Debug.Log("[Fluent] Sending with DSL...");
        relay.Send("Fluent message").ToChildren().Active().Execute();

        Debug.Log("[Result] Both achieve the same routing with different verbosity!");
    }

    void Demo2_RoutingTargets()
    {
        Debug.Log("=== Demo 2: Routing Targets ===");

        Debug.Log("[ToChildren] Direct children only...");
        relay.Send("ToChildren").ToChildren().Execute();

        Debug.Log("[ToDescendants] All descendants (children + grandchildren)...");
        relay.Send("ToDescendants").ToDescendants().Execute();

        Debug.Log("[ToAll] All directions...");
        relay.Send("ToAll").ToAll().Execute();
    }

    void Demo3_TypedValues()
    {
        Debug.Log("=== Demo 3: Typed Values ===");

        relay.Send(42).ToChildren().Execute();
        Debug.Log("[Sent] int: 42");

        relay.Send(3.14f).ToChildren().Execute();
        Debug.Log("[Sent] float: 3.14");

        relay.Send("Hello DSL").ToChildren().Execute();
        Debug.Log("[Sent] string: Hello DSL");

        relay.Send(true).ToChildren().Execute();
        Debug.Log("[Sent] bool: true");

        relay.Send(Vector3.forward).ToChildren().Execute();
        Debug.Log("[Sent] Vector3: forward");
    }

    void Demo4_CombinedFilters()
    {
        Debug.Log("=== Demo 4: Combined Filters ===");

        Debug.Log("[Tag0 Only] Sending to Tag0 responders...");
        relay.Send("Tag0 message").ToChildren().WithTag(MmTag.Tag0).Execute();

        Debug.Log("[Tag1 Only] Sending to Tag1 responders...");
        relay.Send("Tag1 message").ToChildren().WithTag(MmTag.Tag1).Execute();

        Debug.Log("[Active + Tag0] Combined filter...");
        relay.Send("Active Tag0")
            .ToDescendants()
            .Active()
            .WithTag(MmTag.Tag0)
            .Execute();
    }

    void Demo5_AutoExecute()
    {
        Debug.Log("=== Demo 5: Tier 1 Auto-Execute Methods ===");

        Debug.Log("[BroadcastInitialize] Initializing all descendants...");
        relay.BroadcastInitialize();

        Debug.Log("[BroadcastRefresh] Refreshing all descendants...");
        relay.BroadcastRefresh();

        Debug.Log("[BroadcastValue] Broadcasting typed values...");
        relay.BroadcastValue(999);
        relay.BroadcastValue("Auto-execute string");

        Debug.Log("[BroadcastSetActive] Setting active state...");
        relay.BroadcastSetActive(true);
    }

    void DemoT_DelayedExecution()
    {
        Debug.Log("=== Demo T: Delayed Execution ===");
        Debug.Log("[Delay] Message will be sent in 2 seconds...");
        StartCoroutine(DelayedSend());
    }

    IEnumerator DelayedSend()
    {
        yield return new WaitForSeconds(2f);
        relay.Send("Delayed message arrived!").ToDescendants().Execute();
        Debug.Log("[Delay] Message sent after 2 second delay!");
    }

    void DemoY_RepeatingMessages()
    {
        Debug.Log("=== Demo Y: Repeating Messages ===");
        Debug.Log("[Repeat] Cycling through 4 colors...");
        StartCoroutine(CycleColors());
    }

    IEnumerator CycleColors()
    {
        for (int i = 0; i < cycleColors.Length; i++)
        {
            Color c = cycleColors[i];
            relay.Send(new Vector3(c.r, c.g, c.b)).ToDescendants().Execute();
            Debug.Log($"[Repeat] Color {i + 1}/4: {c}");
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("[Repeat] Color cycle complete!");
    }

    void DemoU_ConditionalTrigger()
    {
        Debug.Log("=== Demo U: Conditional Trigger ===");
        Debug.Log("[Conditional] Press SPACE to trigger the message!");
        conditionalReady = true;
    }
}
