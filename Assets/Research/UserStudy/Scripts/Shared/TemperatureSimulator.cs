using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Simulates periodic temperature adjustment requests.
    /// In the Day mode, this sends adjustment values every few seconds.
    /// The HVAC controller should only process these during Day mode.
    /// DO NOT MODIFY THIS FILE.
    /// </summary>
    public class TemperatureSimulator : MonoBehaviour
    {
        [Header("Mercury")]
        public MmRelayNode hvacRelay;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent<float> OnTemperatureRequest;

        [Header("Simulation")]
        public float interval = 3f;
        public float minTemp = 20f;
        public float maxTemp = 25f;

        private float timer;

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                timer = 0f;
                float requestedTemp = Random.Range(minTemp, maxTemp);

                if (hvacRelay != null)
                    hvacRelay.BroadcastValue(requestedTemp);

                OnTemperatureRequest?.Invoke(requestedTemp);
            }
        }
    }
}
