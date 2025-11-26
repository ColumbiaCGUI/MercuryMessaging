using UnityEngine;
using MercuryMessaging;
using TMPro;

namespace UserStudy.SmartHome.Mercury
{
    /// <summary>
    /// Control panel UI for Smart Home scene.
    /// Sends commands to devices via MercuryMessaging and displays status updates.
    /// </summary>
    public class ControlPanel : MmBaseResponder
    {
        [SerializeField] private TextMeshProUGUI statusText;
        private GameObject selectedRoom;

        // Value display references (set by scene builder)
        private TextMeshProUGUI brightnessValueText;
        private TextMeshProUGUI temperatureValueText;

        /// <summary>
        /// Sets the brightness value display reference.
        /// Called by scene builder to wire UI components.
        /// </summary>
        public void SetBrightnessDisplay(TextMeshProUGUI display)
        {
            brightnessValueText = display;
            if (brightnessValueText != null)
                brightnessValueText.text = "100%"; // Initial value
        }

        /// <summary>
        /// Sets the temperature value display reference.
        /// Called by scene builder to wire UI components.
        /// </summary>
        public void SetTemperatureDisplay(TextMeshProUGUI display)
        {
            temperatureValueText = display;
            if (temperatureValueText != null)
                temperatureValueText.text = "22.0°C"; // Initial value
        }

        /// <summary>
        /// Button Handler: Turn off all devices (lights, climate, entertainment).
        /// </summary>
        public void OnAllOffButton()
        {
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.SetActive,
                false,
                new MmMetadataBlock(MmLevelFilter.Parent) // Up to hub, then down to all
            );
        }

        /// <summary>
        /// Button Handler: Turn off only lights (Tag0).
        /// </summary>
        public void OnLightsOffButton()
        {
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.SetActive,
                false,
                new MmMetadataBlock(
                    MmTag.Tag0, // Lights only (tag comes first!)
                    MmLevelFilter.Parent,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                )
            );
        }

        /// <summary>
        /// Button Handler: Turn off only climate devices (Tag1).
        /// </summary>
        public void OnClimateOffButton()
        {
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.SetActive,
                false,
                new MmMetadataBlock(
                    MmTag.Tag1, // Climate devices only (tag comes first!)
                    MmLevelFilter.Parent,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                )
            );
        }

        /// <summary>
        /// Button Handler: Turn off all devices in the currently selected room.
        /// </summary>
        public void OnRoomOffButton()
        {
            if (selectedRoom != null)
            {
                selectedRoom.GetComponent<MmRelayNode>().MmInvoke(
                    MmMethod.SetActive,
                    false,
                    new MmMetadataBlock(MmLevelFilter.Child)
                );
            }
        }

        #region ON Button Handlers

        /// <summary>
        /// Button Handler: Turn ON all devices (lights, climate, entertainment).
        /// </summary>
        public void OnAllOnButton()
        {
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.SetActive,
                true, // Turn ON!
                new MmMetadataBlock(MmLevelFilter.Parent)
            );
        }

        /// <summary>
        /// Button Handler: Turn ON only lights (Tag0).
        /// </summary>
        public void OnLightsOnButton()
        {
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.SetActive,
                true, // Turn ON!
                new MmMetadataBlock(
                    MmTag.Tag0,
                    MmLevelFilter.Parent,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                )
            );
        }

        /// <summary>
        /// Button Handler: Turn ON only climate devices (Tag1).
        /// </summary>
        public void OnClimateOnButton()
        {
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.SetActive,
                true, // Turn ON!
                new MmMetadataBlock(
                    MmTag.Tag1,
                    MmLevelFilter.Parent,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                )
            );
        }

        /// <summary>
        /// Button Handler: Turn ON all devices in the currently selected room.
        /// </summary>
        public void OnRoomOnButton()
        {
            if (selectedRoom != null)
            {
                selectedRoom.GetComponent<MmRelayNode>().MmInvoke(
                    MmMethod.SetActive,
                    true, // Turn ON!
                    new MmMetadataBlock(MmLevelFilter.Child)
                );
            }
        }

        /// <summary>
        /// Button Handler: Turn ON music player (Tag2).
        /// </summary>
        public void OnMusicOnButton()
        {
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.SetActive,
                true,
                new MmMetadataBlock(
                    MmTag.Tag2, // Entertainment
                    MmLevelFilter.Parent,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                )
            );
        }

        /// <summary>
        /// Button Handler: Turn OFF music player (Tag2).
        /// </summary>
        public void OnMusicOffButton()
        {
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.SetActive,
                false,
                new MmMetadataBlock(
                    MmTag.Tag2,
                    MmLevelFilter.Parent,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                )
            );
        }

        #endregion

        #region Advanced Controls

        /// <summary>
        /// Slider Handler: Adjust brightness of all lights (0.0 to 1.0).
        /// Updates local UI display, then sends message to devices.
        /// </summary>
        public void OnBrightnessChanged(float value)
        {
            // Update local UI display (ControlPanel owns its UI elements)
            if (brightnessValueText != null)
                brightnessValueText.text = $"{(value * 100):F0}%";

            // Send message to devices via MercuryMessaging
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.MessageFloat,
                value,
                new MmMetadataBlock(
                    MmTag.Tag0, // Lights only
                    MmLevelFilter.Parent,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                )
            );
        }

        /// <summary>
        /// Slider Handler: Adjust target temperature for thermostats.
        /// Updates local UI display, then sends message to devices.
        /// </summary>
        public void OnTemperatureChanged(float value)
        {
            // Update local UI display (ControlPanel owns its UI elements)
            if (temperatureValueText != null)
                temperatureValueText.text = $"{value:F1}°C";

            // Send message to devices via MercuryMessaging
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.MessageFloat,
                value,
                new MmMetadataBlock(
                    MmTag.Tag1, // Climate devices
                    MmLevelFilter.Parent,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                )
            );
        }

        #endregion

        /// <summary>
        /// Receives status update messages from devices.
        /// </summary>
        protected override void ReceivedMessage(MmMessageString message)
        {
            if (statusText != null)
            {
                statusText.text = message.value;
            }
        }

        /// <summary>
        /// Sets the currently selected room for room-specific control.
        /// Called from UI dropdown.
        /// </summary>
        public void SelectRoom(GameObject room)
        {
            selectedRoom = room;
        }
    }
}
