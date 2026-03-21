using UnityEngine;
using UnityEngine.Events;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Simulates alert generation for a subsystem.
    /// Periodically generates alerts with random severity.
    /// DO NOT MODIFY THIS FILE.
    /// </summary>
    public class AlertSimulator : MonoBehaviour
    {
        [Header("Configuration")]
        public string subsystemName = "HVAC";
        public float minInterval = 2f;
        public float maxInterval = 8f;

        [Header("Alert Messages")]
        public string[] possibleAlerts = new string[]
        {
            "Temperature above threshold",
            "Sensor offline",
            "Maintenance required",
            "Anomaly detected"
        };

        [Header("Events")]
        public UnityEvent<string, int> OnAlertGenerated;

        private float timer;
        private float nextInterval;

        void Start()
        {
            nextInterval = Random.Range(minInterval, maxInterval);
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= nextInterval)
            {
                timer = 0f;
                nextInterval = Random.Range(minInterval, maxInterval);

                string alert = possibleAlerts[Random.Range(0, possibleAlerts.Length)];
                int severity = Random.Range(1, 4);
                OnAlertGenerated?.Invoke(alert, severity);
            }
        }
    }
}
