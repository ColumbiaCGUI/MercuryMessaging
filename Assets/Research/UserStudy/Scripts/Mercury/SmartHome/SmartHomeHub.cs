using UnityEngine;
using MercuryMessaging;

namespace UserStudy.SmartHome.Mercury
{
    /// <summary>
    /// Central hub for Smart Home scene using MercuryMessaging.
    /// Broadcasts mode changes to all devices via simple message passing.
    /// </summary>
    public class SmartHomeHub : MonoBehaviour
    {
        private MmRelayNode relayNode;
        private string currentMode = "Home";

        void Start()
        {
            relayNode = GetComponent<MmRelayNode>();

            if (relayNode == null)
            {
                Debug.LogError("SmartHomeHub requires MmRelayNode component!");
                return;
            }

            // Initialize to Home mode
            SetMode("Home");
        }

        /// <summary>
        /// Sets the home mode and broadcasts the change to all devices.
        /// </summary>
        /// <param name="modeName">Mode name: "Home", "Away", or "Sleep"</param>
        public void SetMode(string modeName)
        {
            currentMode = modeName;

            // Broadcast mode change to all devices
            relayNode.MmInvoke(
                MmMethod.Switch,
                modeName,
                new MmMetadataBlock(MmLevelFilter.Child)
            );

            Debug.Log($"SmartHomeHub: Mode changed to {modeName}");
        }

        /// <summary>
        /// Gets the current mode.
        /// </summary>
        public string CurrentMode => currentMode;
    }
}
