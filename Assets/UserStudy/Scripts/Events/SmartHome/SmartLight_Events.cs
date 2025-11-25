using UnityEngine;
using System.Collections;

namespace UserStudy.SmartHome.Events
{
    /// <summary>
    /// Smart light device using Unity Events approach.
    /// Contrast with Mercury: Requires explicit controller reference.
    /// </summary>
    public class SmartLight_Events : MonoBehaviour, ISmartDevice
    {
        [Header("Controller Reference - Must be manually assigned")]
        [SerializeField] private SmartHomeController controller;

        [Header("Light Components")]
        [SerializeField] private Light lightComponent;
        [SerializeField] private Renderer bulbRenderer;
        [SerializeField] private Material onMaterial;
        [SerializeField] private Material offMaterial;

        private bool isOn = true;
        private float brightness = 1.0f;

        #region ISmartDevice Implementation
        public GameObject GameObject => gameObject;
        public string DeviceName => gameObject.name;
        public DeviceType DeviceType => DeviceType.Light;

        public void SetDeviceActive(bool active)
        {
            isOn = active;
            UpdateLight();

            // Report status to controller (tight coupling)
            if (controller != null)
            {
                controller.OnDeviceStatusChanged(DeviceName, active ? "ON" : "OFF");
            }
        }

        public void SetMode(string modeName)
        {
            switch (modeName)
            {
                case "Home":
                    brightness = 1.0f; // Full brightness
                    isOn = true;
                    break;

                case "Away":
                    isOn = false; // Off when away
                    break;

                case "Sleep":
                    brightness = 0.1f; // Dim for sleep
                    isOn = true;
                    break;
            }

            UpdateLight();
        }
        #endregion

        void Start()
        {
            UpdateLight();
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
            float startBrightness = brightness;
            for (float t = 0; t < 2f; t += Time.deltaTime)
            {
                brightness = Mathf.Lerp(startBrightness, 0, t / 2f);
                UpdateLight();
                yield return null;
            }
            brightness = 0;
            isOn = false;
            UpdateLight();
        }
    }
}
