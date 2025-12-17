// Tutorial 12: VR Experiment - Go/No-Go Task Controller
// This script manages a behavioral experiment with VR support and keyboard fallback.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Examples.Tutorial12
{
    /// <summary>
    /// Go/No-Go task controller for behavioral experiments.
    /// Supports both VR controller input and keyboard fallback.
    /// </summary>
    public class T12_GoNoGoController : MonoBehaviour
    {
        [Header("Task Configuration")]
        [SerializeField] private int totalTrials = 20;
        [SerializeField] private float goTrialRatio = 0.7f; // 70% Go trials
        [SerializeField] private float stimulusDuration = 1.5f;
        [SerializeField] private float interTrialInterval = 1.0f;
        [SerializeField] private float responseWindow = 2.0f;

        [Header("Stimuli")]
        [SerializeField] private GameObject goStimulus;  // Green circle
        [SerializeField] private GameObject noGoStimulus; // Red circle
        [SerializeField] private GameObject fixationCross;

        [Header("Audio Feedback")]
        [SerializeField] private AudioClip correctSound;
        [SerializeField] private AudioClip incorrectSound;
        [SerializeField] private AudioSource audioSource;

        [Header("References")]
        [SerializeField] private MmRelayNode relayNode;

        // Trial data
        private List<TrialData> _trials = new List<TrialData>();
        private int _currentTrialIndex = -1;
        private TrialData _currentTrial;
        private bool _isRunning = false;
        private bool _responseReceived = false;
        private float _stimulusOnsetTime;
        private float _reactionTime;

        // Results
        private List<TrialResult> _results = new List<TrialResult>();

        public struct TrialData
        {
            public int TrialNumber;
            public bool IsGoTrial;
        }

        public struct TrialResult
        {
            public int TrialNumber;
            public bool IsGoTrial;
            public bool Responded;
            public float ReactionTimeMs;
            public bool Correct;
        }

        private void Awake()
        {
            if (relayNode == null)
                relayNode = GetComponent<MmRelayNode>();
        }

        private void Start()
        {
            // Hide stimuli
            if (goStimulus) goStimulus.SetActive(false);
            if (noGoStimulus) noGoStimulus.SetActive(false);
            if (fixationCross) fixationCross.SetActive(true);

            GenerateTrialSequence();

            Debug.Log("[T12] Go/No-Go Task Ready");
            Debug.Log("[T12] Press SPACE to START the experiment");
            Debug.Log("[T12] During trials: Press SPACE or VR trigger for 'Go' response");
        }

        private void Update()
        {
            // Start experiment
            if (!_isRunning && Input.GetKeyDown(KeyCode.Return))
            {
                StartExperiment();
                return;
            }

            // Handle response during active trial
            if (_isRunning && !_responseReceived && _currentTrial.TrialNumber > 0)
            {
                // Keyboard fallback
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    RecordResponse();
                }

                // VR trigger (if available)
                // Note: In actual VR, you'd use XR Input or Standard Library Input messages
#if UNITY_XR_AVAILABLE
                if (UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand).TryGetFeatureValue(
                    UnityEngine.XR.CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
                {
                    RecordResponse();
                }
#endif
            }
        }

        private void GenerateTrialSequence()
        {
            _trials.Clear();
            int goTrials = Mathf.RoundToInt(totalTrials * goTrialRatio);
            int noGoTrials = totalTrials - goTrials;

            // Create trial list
            for (int i = 0; i < goTrials; i++)
                _trials.Add(new TrialData { IsGoTrial = true });
            for (int i = 0; i < noGoTrials; i++)
                _trials.Add(new TrialData { IsGoTrial = false });

            // Shuffle
            for (int i = _trials.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = _trials[i];
                _trials[i] = _trials[j];
                _trials[j] = temp;
            }

            // Assign trial numbers
            for (int i = 0; i < _trials.Count; i++)
            {
                var trial = _trials[i];
                trial.TrialNumber = i + 1;
                _trials[i] = trial;
            }

            Debug.Log($"[T12] Generated {goTrials} Go trials and {noGoTrials} No-Go trials");
        }

        public void StartExperiment()
        {
            if (_isRunning) return;

            _isRunning = true;
            _currentTrialIndex = -1;
            _results.Clear();

            Debug.Log("[T12] Experiment STARTED");

            // Notify experiment start
            relayNode?.BroadcastValue("ExperimentStart");

            StartCoroutine(RunExperiment());
        }

        private IEnumerator RunExperiment()
        {
            yield return new WaitForSeconds(1f); // Initial delay

            while (_currentTrialIndex < _trials.Count - 1)
            {
                _currentTrialIndex++;
                _currentTrial = _trials[_currentTrialIndex];

                yield return RunTrial(_currentTrial);
                yield return new WaitForSeconds(interTrialInterval);
            }

            EndExperiment();
        }

        private IEnumerator RunTrial(TrialData trial)
        {
            _responseReceived = false;

            // Show fixation
            if (fixationCross) fixationCross.SetActive(true);
            if (goStimulus) goStimulus.SetActive(false);
            if (noGoStimulus) noGoStimulus.SetActive(false);

            yield return new WaitForSeconds(0.5f);

            // Show stimulus
            if (fixationCross) fixationCross.SetActive(false);
            if (trial.IsGoTrial)
            {
                if (goStimulus) goStimulus.SetActive(true);
            }
            else
            {
                if (noGoStimulus) noGoStimulus.SetActive(true);
            }

            _stimulusOnsetTime = Time.time;

            // Notify stimulus onset
            relayNode?.BroadcastValue(trial.IsGoTrial ? "GoStimulus" : "NoGoStimulus");

            // Wait for response or timeout
            float elapsed = 0f;
            while (!_responseReceived && elapsed < responseWindow)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Hide stimulus
            if (goStimulus) goStimulus.SetActive(false);
            if (noGoStimulus) noGoStimulus.SetActive(false);

            // Record result
            var result = new TrialResult
            {
                TrialNumber = trial.TrialNumber,
                IsGoTrial = trial.IsGoTrial,
                Responded = _responseReceived,
                ReactionTimeMs = _responseReceived ? _reactionTime * 1000f : -1f,
                Correct = (trial.IsGoTrial && _responseReceived) || (!trial.IsGoTrial && !_responseReceived)
            };

            _results.Add(result);

            // Play feedback sound
            if (audioSource != null)
            {
                audioSource.PlayOneShot(result.Correct ? correctSound : incorrectSound);
            }

            string resultStr = result.Correct ? "CORRECT" : "INCORRECT";
            Debug.Log($"[T12] Trial {trial.TrialNumber}: {(trial.IsGoTrial ? "GO" : "NO-GO")} - {resultStr} - RT: {result.ReactionTimeMs:F0}ms");
        }

        private void RecordResponse()
        {
            if (_responseReceived) return;

            _responseReceived = true;
            _reactionTime = Time.time - _stimulusOnsetTime;

            // Notify response
            relayNode?.BroadcastValue("Response");
        }

        private void EndExperiment()
        {
            _isRunning = false;

            Debug.Log("[T12] ==================== EXPERIMENT COMPLETE ====================");

            // Calculate statistics
            int correct = 0;
            int goCorrect = 0, goTotal = 0;
            int noGoCorrect = 0, noGoTotal = 0;
            float totalRT = 0f;
            int rtCount = 0;

            foreach (var r in _results)
            {
                if (r.Correct) correct++;

                if (r.IsGoTrial)
                {
                    goTotal++;
                    if (r.Correct) goCorrect++;
                    if (r.Responded)
                    {
                        totalRT += r.ReactionTimeMs;
                        rtCount++;
                    }
                }
                else
                {
                    noGoTotal++;
                    if (r.Correct) noGoCorrect++;
                }
            }

            float accuracy = (float)correct / _results.Count * 100f;
            float goAccuracy = goTotal > 0 ? (float)goCorrect / goTotal * 100f : 0f;
            float noGoAccuracy = noGoTotal > 0 ? (float)noGoCorrect / noGoTotal * 100f : 0f;
            float avgRT = rtCount > 0 ? totalRT / rtCount : 0f;

            Debug.Log($"[T12] Overall Accuracy: {accuracy:F1}%");
            Debug.Log($"[T12] Go Trial Accuracy (Hit Rate): {goAccuracy:F1}%");
            Debug.Log($"[T12] No-Go Trial Accuracy (Correct Rejection Rate): {noGoAccuracy:F1}%");
            Debug.Log($"[T12] Average Reaction Time: {avgRT:F0}ms");
            Debug.Log("[T12] =============================================================");

            // Notify experiment end
            relayNode?.BroadcastValue("ExperimentEnd");
        }
    }
}
