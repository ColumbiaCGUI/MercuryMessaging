using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Collections.Generic;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Master controller for the user study.
    /// Manages participant configuration, task sequencing, timing, and data logging.
    ///
    /// Setup:
    /// 1. Attach to a persistent GameObject in the study scene
    /// 2. Configure participant ID and condition order before each session
    /// 3. Press Start to begin the study sequence
    /// </summary>
    public class StudyManager : MonoBehaviour
    {
        [Header("Participant Configuration")]
        public string participantId = "P01";
        public ConditionOrder conditionOrder = ConditionOrder.MercuryFirst;

        [Header("Task Configuration")]
        public float taskTimeLimit = 480f; // 8 minutes per task
        public int totalTasks = 3;

        [Header("UI References")]
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI timerText;
        public Button startButton;
        public Button submitButton;
        public TextMeshProUGUI taskInstructionText;

        [Header("Task Instruction Texts")]
        [TextArea(3, 10)]
        public string[] mercuryTaskInstructions = new string[3];
        [TextArea(3, 10)]
        public string[] eventsTaskInstructions = new string[3];

        // Study state
        private StudyPhase currentPhase = StudyPhase.Setup;
        private int currentConditionIndex = 0; // 0 = first condition, 1 = second
        private int currentTaskIndex = 0;
        private float taskStartTime;
        private float taskElapsedTime;
        private bool taskRunning;
        private string dataPath;

        // Task order (Latin square counterbalancing for 3 tasks: T2, T3, T4)
        private int[][] latinSquare = new int[][]
        {
            new int[] { 0, 1, 2 },
            new int[] { 1, 2, 0 },
            new int[] { 2, 0, 1 }
        };

        private List<TaskResult> results = new List<TaskResult>();

        public enum ConditionOrder { MercuryFirst, EventsFirst }
        public enum StudyPhase { Setup, Training, Tasks, Break, Questionnaire, Complete }

        void Start()
        {
            dataPath = Path.Combine(Application.persistentDataPath, "StudyData", participantId);
            Directory.CreateDirectory(dataPath);

            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
            if (submitButton != null)
            {
                submitButton.onClick.AddListener(OnSubmitClicked);
                submitButton.gameObject.SetActive(false);
            }

            UpdateStatus("Ready. Configure participant ID and press Start.");
        }

        void Update()
        {
            if (taskRunning)
            {
                taskElapsedTime = Time.realtimeSinceStartup - taskStartTime;

                float remaining = taskTimeLimit - taskElapsedTime;
                if (timerText != null)
                {
                    int minutes = Mathf.FloorToInt(remaining / 60f);
                    int seconds = Mathf.FloorToInt(remaining % 60f);
                    timerText.text = $"{minutes:00}:{seconds:00}";
                    timerText.color = remaining < 60f ? Color.red : Color.white;
                }

                if (taskElapsedTime >= taskTimeLimit)
                {
                    EndTask(false); // Timed out
                }
            }
        }

        void OnStartClicked()
        {
            currentPhase = StudyPhase.Training;
            currentConditionIndex = 0;
            currentTaskIndex = 0;
            UpdateStatus($"Training Phase — {GetCurrentConditionName()}");
            startButton.gameObject.SetActive(false);

            // Training is manual — experimenter advances when ready
            ShowTrainingInstructions();
        }

        public void StartTaskPhase()
        {
            currentPhase = StudyPhase.Tasks;
            currentTaskIndex = 0;
            StartNextTask();
        }

        public void StartNextTask()
        {
            if (currentTaskIndex >= totalTasks)
            {
                EndCondition();
                return;
            }

            // Get task order from Latin square
            int participantNum = int.Parse(participantId.Replace("P", ""));
            int[] taskOrder = latinSquare[(participantNum - 1) % latinSquare.Length];
            int actualTask = taskOrder[currentTaskIndex];

            string condition = GetCurrentConditionName();
            string[] instructions = condition == "Mercury" ? mercuryTaskInstructions : eventsTaskInstructions;

            if (taskInstructionText != null && actualTask < instructions.Length)
                taskInstructionText.text = instructions[actualTask];

            UpdateStatus($"{condition} — Task {currentTaskIndex + 1}/{totalTasks} (T{actualTask + 2})");

            taskRunning = true;
            taskStartTime = Time.realtimeSinceStartup;
            taskElapsedTime = 0f;

            if (submitButton != null)
                submitButton.gameObject.SetActive(true);

            LogEvent("TASK_START", $"condition={condition},task=T{actualTask + 2},index={currentTaskIndex}");
        }

        void OnSubmitClicked()
        {
            if (taskRunning)
                EndTask(true); // Participant submitted
        }

        void EndTask(bool submitted)
        {
            taskRunning = false;
            float completionTime = taskElapsedTime;

            int participantNum = int.Parse(participantId.Replace("P", ""));
            int[] taskOrder = latinSquare[(participantNum - 1) % latinSquare.Length];
            int actualTask = taskOrder[currentTaskIndex];

            var result = new TaskResult
            {
                participantId = participantId,
                condition = GetCurrentConditionName(),
                taskId = $"T{actualTask + 2}",
                taskIndex = currentTaskIndex,
                completionTime = completionTime,
                submitted = submitted,
                timedOut = !submitted,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            results.Add(result);

            LogEvent("TASK_END", $"condition={result.condition},task={result.taskId}," +
                $"time={completionTime:F2},submitted={submitted}");

            if (submitButton != null)
                submitButton.gameObject.SetActive(false);

            currentTaskIndex++;

            UpdateStatus(submitted
                ? $"Task submitted ({completionTime:F1}s). Press Start Next Task when ready."
                : $"Task timed out ({taskTimeLimit}s). Press Start Next Task when ready.");
        }

        void EndCondition()
        {
            string condition = GetCurrentConditionName();
            UpdateStatus($"{condition} condition complete. Administer questionnaires (NASA-TLX, SUS).");
            currentPhase = StudyPhase.Questionnaire;
            LogEvent("CONDITION_END", $"condition={condition}");
        }

        public void StartSecondCondition()
        {
            currentConditionIndex = 1;
            currentTaskIndex = 0;
            currentPhase = StudyPhase.Training;
            UpdateStatus($"Training Phase — {GetCurrentConditionName()}");
            ShowTrainingInstructions();
        }

        public void EndStudy()
        {
            currentPhase = StudyPhase.Complete;
            SaveResults();
            UpdateStatus($"Study complete for {participantId}. Data saved to {dataPath}");
            LogEvent("STUDY_END", $"participant={participantId},totalTasks={results.Count}");
        }

        string GetCurrentConditionName()
        {
            bool mercuryFirst = conditionOrder == ConditionOrder.MercuryFirst;
            if (currentConditionIndex == 0)
                return mercuryFirst ? "Mercury" : "UnityEvents";
            else
                return mercuryFirst ? "UnityEvents" : "Mercury";
        }

        void ShowTrainingInstructions()
        {
            string condition = GetCurrentConditionName();
            if (taskInstructionText != null)
                taskInstructionText.text = $"TRAINING: {condition}\n\nFollow the training exercises. " +
                    $"The experimenter will advance to the task phase when training is complete.";
        }

        void UpdateStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
            Debug.Log($"[StudyManager] {message}");
        }

        void LogEvent(string eventType, string data)
        {
            string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{eventType},{data}";
            string logFile = Path.Combine(dataPath, "study_log.csv");

            if (!File.Exists(logFile))
                File.WriteAllText(logFile, "timestamp,event,data\n");

            File.AppendAllText(logFile, logLine + "\n");
        }

        void SaveResults()
        {
            string resultFile = Path.Combine(dataPath, "task_results.csv");
            using (var writer = new StreamWriter(resultFile))
            {
                writer.WriteLine("participant,condition,task,taskIndex,completionTime,submitted,timedOut,timestamp");
                foreach (var r in results)
                {
                    writer.WriteLine($"{r.participantId},{r.condition},{r.taskId},{r.taskIndex}," +
                        $"{r.completionTime:F2},{r.submitted},{r.timedOut},{r.timestamp}");
                }
            }
            Debug.Log($"[StudyManager] Results saved to {resultFile}");
        }
    }

    [Serializable]
    public class TaskResult
    {
        public string participantId;
        public string condition;
        public string taskId;
        public int taskIndex;
        public float completionTime;
        public bool submitted;
        public bool timedOut;
        public string timestamp;
    }
}
