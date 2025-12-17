using UnityEngine;
using MercuryMessaging;

namespace UserStudy.SmartHome.Mercury
{
    /// <summary>
    /// Smart thermostat device that simulates temperature control.
    /// Responds to mode changes with different temperature targets.
    /// </summary>
    public class Thermostat : MmBaseResponder
    {
        [SerializeField] private float currentTemp = 20f;
        [SerializeField] private float targetTemp = 22f;
        [SerializeField] private float heatingRate = 0.5f;

        private bool isHeating = true;
        #pragma warning disable CS0414
        private bool isNightMode = false;
        #pragma warning restore CS0414

        // Visual feedback components
        private TMPro.TextMeshPro tempDisplay;
        private Renderer visualRenderer;
        private Material heatingMaterial;
        private Material idleMaterial;

        public override void Awake()
        {
            base.Awake();

            // Set tag for filtering (Tag1 = Climate)
            Tag = MmTag.Tag1;
            TagCheckEnabled = true;

            // Get visual feedback components
            tempDisplay = GetComponentInChildren<TMPro.TextMeshPro>();
            visualRenderer = GetComponentInChildren<Renderer>();

            // Create materials for heating status feedback
            heatingMaterial = new Material(Shader.Find("Standard"));
            heatingMaterial.color = new Color(1f, 0.5f, 0f); // Orange for heating

            idleMaterial = new Material(Shader.Find("Standard"));
            idleMaterial.color = new Color(0.3f, 0.5f, 0.7f); // Blue-gray for idle

            // Set initial material to idle
            if (visualRenderer != null)
                visualRenderer.material = idleMaterial;
        }

        new void Update()
        {
            // Update temperature display in real-time
            if (tempDisplay != null)
                tempDisplay.text = $"{currentTemp:F1}°C";

            if (isHeating && currentTemp < targetTemp)
            {
                currentTemp += heatingRate * Time.deltaTime;

                if (currentTemp >= targetTemp)
                {
                    currentTemp = targetTemp;
                    OnTargetReached();
                }
            }
        }

        /// <summary>
        /// Handles SetActive messages (on/off commands).
        /// </summary>
        public override void SetActive(bool active)
        {
            isHeating = active;

            // Update visual feedback: orange when heating, blue when off
            if (visualRenderer != null)
                visualRenderer.material = active ? heatingMaterial : idleMaterial;

            // Report status
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.MessageString,
                $"{gameObject.name}: {(active ? "Heating" : "Off")}",
                new MmMetadataBlock(MmLevelFilter.Parent)
            );
        }

        /// <summary>
        /// Handles Switch messages (mode changes).
        /// Modes: "Home", "Away", "Sleep"
        /// </summary>
        protected override void Switch(string modeName)
        {
            switch (modeName)
            {
                case "Home": // Home mode
                    targetTemp = 22f;
                    isNightMode = false;
                    break;

                case "Away": // Away mode
                    targetTemp = 18f; // Energy saving
                    break;

                case "Sleep": // Sleep mode
                    targetTemp = 19f; // Night mode
                    isNightMode = true;
                    break;
            }

            isHeating = true;
        }

        /// <summary>
        /// Handles float messages for temperature control (16-30°C recommended).
        /// </summary>
        protected override void ReceivedMessage(MmMessageFloat message)
        {
            targetTemp = Mathf.Clamp(message.value, 16f, 30f); // Clamp to reasonable range
            isHeating = true;

            // Report temperature change
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.MessageString,
                $"{gameObject.name}: Target set to {targetTemp:F1}°C",
                new MmMetadataBlock(MmLevelFilter.Parent)
            );
        }

        /// <summary>
        /// Called when target temperature is reached.
        /// Reports status to control panel.
        /// </summary>
        private void OnTargetReached()
        {
            // Turn blue when target is reached (stop heating visually)
            if (visualRenderer != null)
                visualRenderer.material = idleMaterial;

            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.MessageString,
                $"{gameObject.name}: Target temperature reached ({targetTemp}°C)",
                new MmMetadataBlock(MmLevelFilter.Parent)
            );
        }
    }
}
