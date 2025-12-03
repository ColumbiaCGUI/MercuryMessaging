// Copyright (c) 2017-2025, Columbia University
// DSL Tutorial 1: Basic Fluent API Usage
// Demonstrates the difference between traditional API and Fluent DSL

using UnityEngine;
using MercuryMessaging;


/// <summary>
/// Tutorial: Basic Fluent API Usage
///
/// This script demonstrates how to use the Fluent DSL for cleaner, more
/// readable message sending. Compare the traditional 7-line approach with
/// the 1-line Fluent DSL equivalent.
///
/// Setup:
/// 1. Add this script to a GameObject with MmRelayNode
/// 2. Create child GameObjects with MmBaseResponder components
/// 3. Press number keys 1-5 to test different messaging patterns
/// </summary>
public class DSLBasicDemo : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
        if (relay == null)
        {
            Debug.LogError("[DSL Tutorial] MmRelayNode required on this GameObject!");
            return;
        }

        Debug.Log("[DSL Tutorial] Basic Demo Ready. Press 1-5 to test messaging patterns.");
    }

    void Update()
    {
        if (relay == null) return;

        // Demo 1: Compare Traditional vs Fluent API
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Demo1_TraditionalVsFluent();
        }

        // Demo 2: Different routing targets
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Demo2_RoutingTargets();
        }

        // Demo 3: Sending typed values
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Demo3_TypedValues();
        }

        // Demo 4: Combining filters
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Demo4_CombiningFilters();
        }

        // Demo 5: Auto-execute convenience methods
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Demo5_AutoExecuteMethods();
        }
    }

    /// <summary>
    /// Demo 1: Traditional API (7 lines) vs Fluent DSL (1 line)
    /// Both do exactly the same thing!
    /// </summary>
    void Demo1_TraditionalVsFluent()
    {
        Debug.Log("=== Demo 1: Traditional vs Fluent ===");

        // Traditional API - 7 lines of code
        // relay.MmInvoke(
        //     MmMethod.MessageString,
        //     "Hello",
        //     new MmMetadataBlock(
        //         MmLevelFilter.Child,
        //         MmActiveFilter.Active,
        //         MmSelectedFilter.All,
        //         MmNetworkFilter.Local
        //     )
        // );

        // Fluent DSL - 1 line, same result!
        relay.Send("Hello").ToChildren().Active().Execute();

        Debug.Log("Sent 'Hello' to children using Fluent DSL");
    }

    /// <summary>
    /// Demo 2: Different routing targets available in Fluent DSL
    /// </summary>
    void Demo2_RoutingTargets()
    {
        Debug.Log("=== Demo 2: Routing Targets ===");

        // Send to direct children only
        relay.Send("ToChildren").ToChildren().Execute();
        Debug.Log("Sent to direct children");

        // Send to all descendants (children, grandchildren, etc.)
        relay.Send("ToDescendants").ToDescendants().Execute();
        Debug.Log("Sent to all descendants");

        // Send to parents/ancestors
        relay.Send("ToParents").ToParents().Execute();
        Debug.Log("Sent to parents");

        // Send bidirectionally (parents AND children)
        relay.Send("ToAll").ToAll().Execute();
        Debug.Log("Sent bidirectionally");
    }

    /// <summary>
    /// Demo 3: Sending different typed values
    /// </summary>
    void Demo3_TypedValues()
    {
        Debug.Log("=== Demo 3: Typed Values ===");

        // Send an integer
        relay.Send(42).ToChildren().Execute();
        Debug.Log("Sent int: 42");

        // Send a float
        relay.Send(3.14f).ToChildren().Execute();
        Debug.Log("Sent float: 3.14");

        // Send a string
        relay.Send("Hello World").ToChildren().Execute();
        Debug.Log("Sent string: Hello World");

        // Send a boolean
        relay.Send(true).ToChildren().Execute();
        Debug.Log("Sent bool: true");

        // Send a Vector3
        relay.Send(transform.position).ToChildren().Execute();
        Debug.Log("Sent Vector3: " + transform.position);
    }

    /// <summary>
    /// Demo 4: Combining multiple filters
    /// </summary>
    void Demo4_CombiningFilters()
    {
        Debug.Log("=== Demo 4: Combining Filters ===");

        // Chain multiple filters together
        relay.Send("FilteredMessage")
            .ToDescendants()          // All descendants
            .Active()                  // Only active GameObjects
            .WithTag(MmTag.Tag0)      // Only Tag0 responders
            .Execute();

        Debug.Log("Sent filtered message to active Tag0 descendants");
    }

    /// <summary>
    /// Demo 5: Auto-execute convenience methods (no .Execute() needed)
    /// </summary>
    void Demo5_AutoExecuteMethods()
    {
        Debug.Log("=== Demo 5: Auto-Execute Methods ===");

        // These methods execute immediately - no .Execute() needed!

        // Broadcast DOWN to descendants
        relay.BroadcastInitialize();
        Debug.Log("Broadcast Initialize");

        relay.BroadcastRefresh();
        Debug.Log("Broadcast Refresh");

        relay.BroadcastSetActive(true);
        Debug.Log("Broadcast SetActive(true)");

        relay.BroadcastValue(100);
        Debug.Log("Broadcast Value: 100");

        // Notify UP to parents
        relay.NotifyComplete();
        Debug.Log("Notify Complete to parents");

        relay.NotifyValue("Status OK");
        Debug.Log("Notify Value to parents");
    }
}
