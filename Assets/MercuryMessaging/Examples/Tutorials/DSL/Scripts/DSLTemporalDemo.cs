// Copyright (c) 2017-2025, Columbia University
// DSL Tutorial 2: Temporal Extensions
// Demonstrates time-based messaging patterns

using UnityEngine;
using MercuryMessaging;

using System.Collections;

/// <summary>
/// Tutorial: Temporal Extensions
///
/// This script demonstrates time-based messaging patterns including
/// delayed execution, repeating messages, and conditional triggers.
///
/// Setup:
/// 1. Add this script to a GameObject with MmRelayNode
/// 2. Create child GameObjects with ColorResponder components
/// 3. Press keys T, Y, U to test temporal patterns
/// </summary>
public class DSLTemporalDemo : MonoBehaviour
{
    private MmRelayNode relay;
    private bool conditionMet = false;
    private Coroutine repeatingCoroutine;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
        if (relay == null)
        {
            Debug.LogError("[DSL Tutorial] MmRelayNode required on this GameObject!");
            return;
        }

        Debug.Log("[DSL Tutorial] Temporal Demo Ready. Press T, Y, U to test temporal patterns.");
    }

    void Update()
    {
        if (relay == null) return;

        // Demo: Delayed execution
        if (Input.GetKeyDown(KeyCode.T))
        {
            DemoDelayedExecution();
        }

        // Demo: Repeating messages
        if (Input.GetKeyDown(KeyCode.Y))
        {
            DemoRepeatingMessages();
        }

        // Demo: Conditional trigger
        if (Input.GetKeyDown(KeyCode.U))
        {
            DemoConditionalTrigger();
        }

        // Toggle condition with Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            conditionMet = !conditionMet;
            Debug.Log("Condition toggled: " + conditionMet);
        }
    }

    /// <summary>
    /// Demo: Execute a message after a delay
    /// </summary>
    void DemoDelayedExecution()
    {
        Debug.Log("=== Delayed Execution Demo ===");
        Debug.Log("Sending 'red' in 2 seconds...");

        // Send a delayed message using coroutine
        StartCoroutine(DelayedSend());
    }

    IEnumerator DelayedSend()
    {
        // First, set to blue immediately
        relay.Send("blue").ToDescendants().Execute();
        Debug.Log("Set to blue immediately");

        // Wait 2 seconds
        yield return new WaitForSeconds(2f);

        // Then change to red
        relay.Send("red").ToDescendants().Execute();
        Debug.Log("Changed to red after 2 seconds");
    }

    /// <summary>
    /// Demo: Send repeating messages at intervals
    /// </summary>
    void DemoRepeatingMessages()
    {
        Debug.Log("=== Repeating Messages Demo ===");

        if (repeatingCoroutine != null)
        {
            StopCoroutine(repeatingCoroutine);
            Debug.Log("Stopped previous repeating messages");
        }

        repeatingCoroutine = StartCoroutine(RepeatingSend());
    }

    IEnumerator RepeatingSend()
    {
        string[] colors = { "red", "green", "blue", "yellow" };
        int repeatCount = 8;

        Debug.Log("Cycling through colors " + repeatCount + " times (every 0.5s)");

        for (int i = 0; i < repeatCount; i++)
        {
            string color = colors[i % colors.Length];
            relay.Send(color).ToDescendants().Execute();
            Debug.Log("Sent: " + color + " (" + (i + 1) + "/" + repeatCount + ")");

            yield return new WaitForSeconds(0.5f);
        }

        // Reset at the end
        relay.Send("reset").ToDescendants().Execute();
        Debug.Log("Repeating sequence complete, reset to original");
        repeatingCoroutine = null;
    }

    /// <summary>
    /// Demo: Execute when a condition becomes true
    /// </summary>
    void DemoConditionalTrigger()
    {
        Debug.Log("=== Conditional Trigger Demo ===");
        Debug.Log("Waiting for condition... Press SPACE to toggle condition");

        StartCoroutine(WaitForCondition());
    }

    IEnumerator WaitForCondition()
    {
        // Set to gray while waiting
        relay.Send("white").ToDescendants().Execute();

        // Wait until condition is met
        float timeout = 10f;
        float elapsed = 0f;

        while (!conditionMet && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (conditionMet)
        {
            relay.Send("green").ToDescendants().Execute();
            Debug.Log("Condition met! Changed to green");
            conditionMet = false; // Reset for next demo
        }
        else
        {
            relay.Send("red").ToDescendants().Execute();
            Debug.Log("Timeout! Condition not met. Changed to red");
        }
    }

    void OnDisable()
    {
        if (repeatingCoroutine != null)
        {
            StopCoroutine(repeatingCoroutine);
        }
    }
}
