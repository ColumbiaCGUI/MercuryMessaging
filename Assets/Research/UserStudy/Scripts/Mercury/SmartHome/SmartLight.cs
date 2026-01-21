using UnityEngine;
using System.Collections;
using MercuryMessaging;

namespace UserStudy.SmartHome.Mercury
{
    /// <summary>
    /// Smart light device that responds to MercuryMessaging commands.
    /// Supports on/off, brightness control, and mode-based behavior.
    /// </summary>
    public class SmartLight : MmBaseResponder
    {
        [SerializeField] private Light lightComponent;
        [SerializeField] private Renderer bulbRenderer;
        [SerializeField] private Material onMaterial;
        [SerializeField] private Material offMaterial;

        private bool isOn = true;
        private float brightness = 1.0f;

        public override void Awake()
        {
            base.Awake();

            // Set tag for filtering (Tag0 = Lights)
            Tag = MmTag.Tag0;
            TagCheckEnabled = true;
        }

        /// <summary>
        /// Handles SetActive messages (on/off commands).
        /// </summary>
        public override void SetActive(bool active)
        {
            isOn = active;
            UpdateLight();

            // Report status to control panel
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.MessageString,
                $"{gameObject.name}: {(active ? "ON" : "OFF")}",
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
                    brightness = 1.0f; // Full brightness
                    isOn = true;
                    break;

                case "Away": // Away mode
                    isOn = false; // Off when away
                    break;

                case "Sleep": // Sleep mode
                    brightness = 0.1f; // Dim for sleep
                    isOn = true;
                    break;
            }

            UpdateLight();
        }

        /// <summary>
        /// Handles float messages for brightness control (0.0 to 1.0).
        /// </summary>
        protected override void ReceivedMessage(MmMessageFloat message)
        {
            brightness = Mathf.Clamp01(message.value); // Clamp to 0-1 range
            if (brightness > 0.01f && !isOn)
            {
                isOn = true; // Turn on if brightness is above threshold
            }
            UpdateLight();

            // Report brightness change
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.MessageString,
                $"{gameObject.name}: Brightness {(brightness * 100):F0}%",
                new MmMetadataBlock(MmLevelFilter.Parent)
            );
        }

        /// <summary>
        /// Updates the light's visual appearance based on current state.
        /// </summary>
        private void UpdateLight()
        {
            if (lightComponent != null)
            {
                lightComponent.enabled = isOn;
                lightComponent.intensity = isOn ? brightness : 0;
            }

            if (bulbRenderer != null)
            {
                bulbRenderer.material = isOn ? onMaterial : offMaterial;
            }
        }

        /// <summary>
        /// Fades out the light over 2 seconds (used for transitions).
        /// </summary>
        private IEnumerator FadeOut()
        {
            float startVol = brightness;
            for (float t = 0; t < 2f; t += Time.deltaTime)
            {
                brightness = Mathf.Lerp(startVol, 0, t / 2f);
                UpdateLight();
                yield return null;
            }
            brightness = 0;
            isOn = false;
            UpdateLight();
        }
    }
}
