using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace UserStudy.SmartHome.Events
{
    /// <summary>
    /// Central controller for Smart Home scene using Unity Events approach.
    /// Manages all devices through explicit lists and references.
    /// Contrast with Mercury: Requires manual device registration and iteration.
    /// </summary>
    public class SmartHomeController : MonoBehaviour
    {
        [Header("Device Lists - Must be manually populated")]
        [SerializeField] private List<ISmartDevice> allDevices = new List<ISmartDevice>();
        [SerializeField] private List<ISmartDevice> lightDevices = new List<ISmartDevice>();
        [SerializeField] private List<ISmartDevice> climateDevices = new List<ISmartDevice>();
        [SerializeField] private List<ISmartDevice> entertainmentDevices = new List<ISmartDevice>();

        [Header("Room References - Must be manually populated")]
        [SerializeField] private List<ISmartDevice> bedroomDevices = new List<ISmartDevice>();
        [SerializeField] private List<ISmartDevice> kitchenDevices = new List<ISmartDevice>();
        [SerializeField] private List<ISmartDevice> livingRoomDevices = new List<ISmartDevice>();

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI statusText;

        private string currentMode = "Home";
        private List<ISmartDevice> selectedRoomDevices;

        void Start()
        {
            // Initialize to Home mode
            SetMode("Home");
        }

        /// <summary>
        /// Button Handler: Turn off all devices.
        /// Contrast with Mercury: Must manually iterate through device list.
        /// </summary>
        public void OnAllOffButton()
        {
            foreach (var device in allDevices)
            {
                if (device != null)
                {
                    device.SetDeviceActive(false);
                }
            }

            UpdateStatus("All devices turned off");
        }

        /// <summary>
        /// Button Handler: Turn off only lights.
        /// Contrast with Mercury: Must manually iterate through filtered list.
        /// </summary>
        public void OnLightsOffButton()
        {
            foreach (var light in lightDevices)
            {
                if (light != null)
                {
                    light.SetDeviceActive(false);
                }
            }

            UpdateStatus("All lights turned off");
        }

        /// <summary>
        /// Button Handler: Turn off only climate devices.
        /// Contrast with Mercury: Must manually iterate through filtered list.
        /// </summary>
        public void OnClimateOffButton()
        {
            foreach (var climate in climateDevices)
            {
                if (climate != null)
                {
                    climate.SetDeviceActive(false);
                }
            }

            UpdateStatus("Climate devices turned off");
        }

        /// <summary>
        /// Button Handler: Turn off devices in selected room.
        /// Contrast with Mercury: Must track selected room manually.
        /// </summary>
        public void OnRoomOffButton()
        {
            if (selectedRoomDevices != null)
            {
                foreach (var device in selectedRoomDevices)
                {
                    if (device != null)
                    {
                        device.SetDeviceActive(false);
                    }
                }

                UpdateStatus("Selected room devices turned off");
            }
        }

        /// <summary>
        /// Sets the current mode and updates all devices.
        /// Contrast with Mercury: Must manually broadcast to all devices.
        /// </summary>
        public void SetMode(string modeName)
        {
            currentMode = modeName;

            foreach (var device in allDevices)
            {
                if (device != null)
                {
                    device.SetMode(modeName);
                }
            }

            UpdateStatus($"Mode changed to: {modeName}");
        }

        /// <summary>
        /// Home mode button handler.
        /// </summary>
        public void OnHomeModeButton()
        {
            SetMode("Home");
        }

        /// <summary>
        /// Away mode button handler.
        /// </summary>
        public void OnAwayModeButton()
        {
            SetMode("Away");
        }

        /// <summary>
        /// Sleep mode button handler.
        /// </summary>
        public void OnSleepModeButton()
        {
            SetMode("Sleep");
        }

        /// <summary>
        /// Selects the bedroom for room-specific control.
        /// Contrast with Mercury: Must maintain room selection state.
        /// </summary>
        public void SelectBedroomRoom()
        {
            selectedRoomDevices = bedroomDevices;
            UpdateStatus("Bedroom selected");
        }

        /// <summary>
        /// Selects the kitchen for room-specific control.
        /// </summary>
        public void SelectKitchenRoom()
        {
            selectedRoomDevices = kitchenDevices;
            UpdateStatus("Kitchen selected");
        }

        /// <summary>
        /// Selects the living room for room-specific control.
        /// </summary>
        public void SelectLivingRoomRoom()
        {
            selectedRoomDevices = livingRoomDevices;
            UpdateStatus("Living Room selected");
        }

        /// <summary>
        /// Receives status updates from devices.
        /// Contrast with Mercury: Devices must call this method directly (tight coupling).
        /// </summary>
        public void OnDeviceStatusChanged(string deviceName, string status)
        {
            UpdateStatus($"{deviceName}: {status}");
        }

        /// <summary>
        /// Updates the status text display.
        /// </summary>
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        #region Manual Device Registration - Required for Unity Events approach
        /// <summary>
        /// Registers a device in all appropriate lists.
        /// Contrast with Mercury: Automatic registration through hierarchy.
        /// NOTE: This must be called manually or done via Inspector assignment.
        /// </summary>
        public void RegisterDevice(ISmartDevice device, string roomName)
        {
            if (device == null) return;

            // Add to all devices list
            if (!allDevices.Contains(device))
            {
                allDevices.Add(device);
            }

            // Add to type-specific list
            switch (device.DeviceType)
            {
                case DeviceType.Light:
                    if (!lightDevices.Contains(device))
                        lightDevices.Add(device);
                    break;
                case DeviceType.Climate:
                    if (!climateDevices.Contains(device))
                        climateDevices.Add(device);
                    break;
                case DeviceType.Entertainment:
                    if (!entertainmentDevices.Contains(device))
                        entertainmentDevices.Add(device);
                    break;
            }

            // Add to room-specific list
            switch (roomName)
            {
                case "Bedroom":
                    if (!bedroomDevices.Contains(device))
                        bedroomDevices.Add(device);
                    break;
                case "Kitchen":
                    if (!kitchenDevices.Contains(device))
                        kitchenDevices.Add(device);
                    break;
                case "LivingRoom":
                    if (!livingRoomDevices.Contains(device))
                        livingRoomDevices.Add(device);
                    break;
            }
        }

        /// <summary>
        /// Unregisters a device from all lists.
        /// Contrast with Mercury: Automatic cleanup through component lifecycle.
        /// </summary>
        public void UnregisterDevice(ISmartDevice device)
        {
            allDevices.Remove(device);
            lightDevices.Remove(device);
            climateDevices.Remove(device);
            entertainmentDevices.Remove(device);
            bedroomDevices.Remove(device);
            kitchenDevices.Remove(device);
            livingRoomDevices.Remove(device);
        }
        #endregion
    }
}
