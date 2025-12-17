using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;

/// <summary>
/// Tutorial 9: Trial responder for experiment tasks.
/// Handles trial execution, stimulus presentation, and response recording.
///
/// Keyboard Controls:
/// Space - Record response during trial
/// </summary>
public class T9_TrialResponder : MmBaseResponder
{
    [Header("Trial Settings")]
    [SerializeField] private float stimulusDuration = 0.5f;
    [SerializeField] private float maxResponseTime = 5f;

    [Header("UI References (Optional)")]
    [SerializeField] private GameObject stimulus;
    [SerializeField] private GameObject readyPrompt;

    // Trial state
    private float trialStartTime;
    private bool awaitingResponse;
    private bool responded;
    private T9_MyTaskInfo currentTask;

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("[Trial] Initialized - awaiting task info");
    }

    /// <summary>
    /// Receives task info from experiment manager.
    /// </summary>
    protected override void ReceivedMessage(MmMessageSerializable msg)
    {
        // Check if this is task info
        if (msg.value is T9_MyTaskInfo taskInfo)
        {
            currentTask = taskInfo;
            StartTrial();
        }
    }

    void StartTrial()
    {
        if (currentTask == null) return;

        Debug.Log($"[Trial] Starting: Trial {currentTask.TrialNumber}, Condition: {currentTask.Condition}");

        responded = false;
        awaitingResponse = true;
        trialStartTime = Time.time;

        // Show stimulus
        if (stimulus != null)
        {
            stimulus.SetActive(true);
            Invoke(nameof(HideStimulus), stimulusDuration);
        }

        // Setup response timeout
        float timeLimit = currentTask.TimeLimit > 0 ? currentTask.TimeLimit : maxResponseTime;
        Invoke(nameof(EndResponsePeriod), timeLimit);
    }

    void HideStimulus()
    {
        if (stimulus != null)
        {
            stimulus.SetActive(false);
        }
    }

    new void Update()
    {
        if (awaitingResponse && Input.GetKeyDown(KeyCode.Space))
        {
            RecordResponse();
        }
    }

    void RecordResponse()
    {
        if (!awaitingResponse) return;

        responded = true;
        awaitingResponse = false;
        float reactionTime = Time.time - trialStartTime;

        Debug.Log($"[Trial] Response recorded - RT: {reactionTime:F3}s");

        // Cancel timeout
        CancelInvoke(nameof(EndResponsePeriod));

        // Notify parent of response
        NotifyTrialComplete(true, reactionTime);
    }

    void EndResponsePeriod()
    {
        if (!awaitingResponse) return;

        awaitingResponse = false;

        if (!responded)
        {
            Debug.Log("[Trial] No response - timeout");
            NotifyTrialComplete(false, maxResponseTime);
        }
    }

    void NotifyTrialComplete(bool didRespond, float reactionTime)
    {
        // Send result to parent
        string result = didRespond ? $"Response:{reactionTime:F3}" : "NoResponse";
        this.Send(result).ToParents().Execute();
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);

        if (!active)
        {
            // Cancel any pending invokes when deactivated
            CancelInvoke();
            awaitingResponse = false;
        }
    }

    public override void Refresh(List<MmTransform> transformList)
    {
        Debug.Log("[Trial] Refreshed");
    }
}
