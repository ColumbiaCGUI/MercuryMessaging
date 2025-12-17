using UnityEngine;

namespace UserStudy.SmartHome.Events
{
    /// <summary>
    /// Smart thermostat device using Unity Events approach.
    /// Contrast with Mercury: Requires explicit controller reference.
    /// </summary>
    public class Thermostat_Events : MonoBehaviour, ISmartDevice
    {
        [Header("Controller Reference - Must be manually assigned")]
        [SerializeField] private SmartHomeController controller;

        [Header("Thermostat Settings")]
        [SerializeField] private float currentTemp = 20f;
        [SerializeField] private float targetTemp = 22f;
        [SerializeField] private float heatingRate = 0.5f;

        private bool isHeating = true;
        #pragma warning disable CS0414
        private bool isNightMode = false;
        #pragma warning restore CS0414

        #region ISmartDevice Implementation
        public GameObject GameObject => gameObject;
        public string DeviceName => gameObject.name;
        public DeviceType DeviceType => DeviceType.Climate;

        public void SetDeviceActive(bool active)
        {
            isHeating = active;

            // Report status to controller (tight coupling)
            if (controller != null)
            {
                controller.OnDeviceStatusChanged(DeviceName, active ? "Heating" : "Off");
            }
        }

        public void SetMode(string modeName)
        {
            switch (modeName)
            {
                case "Home":
                    targetTemp = 22f;
                    isNightMode = false;
                    break;

                case "Away":
                    targetTemp = 18f; // Energy saving
                    break;

                case "Sleep":
                    targetTemp = 19f; // Night mode
                    isNightMode = true;
                    break;
            }

            isHeating = true;
        }
        #endregion

        void Update()
        {
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
        /// Called when target temperature is reached.
        /// Reports status to controller.
        /// </summary>
        private void OnTargetReached()
        {
            if (controller != null)
            {
                controller.OnDeviceStatusChanged(DeviceName, $"Target temperature reached ({targetTemp}°C)");
            }
        }
    }
}
