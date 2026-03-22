using UnityEngine;
using TMPro;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// HVAC Controller for Mode-Switch scenario.
    ///
    /// BUG: When the facility switches to Night mode, the HVAC daytime
    /// adjustment routine still fires. The thermostat adjustments that
    /// should only happen during Day mode are executing during Night mode.
    ///
    /// TASK: Find and fix the bug so that HVAC adjustments only occur
    /// during Day mode. When in Night mode, the HVAC should enter
    /// energy-saving defaults (setpoint = 18°C) and ignore adjustment commands.
    ///
    /// HINT: Check how this responder handles the Switch message
    /// and whether it properly gates behavior based on current mode.
    /// </summary>
    public class HvacController_Buggy : MmBaseResponder
    {
        [Header("HVAC Settings")]
        public float daySetpoint = 22f;
        public float nightSetpoint = 18f;

        [Header("UI")]
        public TextMeshProUGUI statusText;

        private float currentSetpoint;
        private string currentMode = "Day";
        // BUG: This flag is set but never checked in AdjustTemperature
        private bool isActive = true;

        public override void Start()
        {
            base.Start();
            currentSetpoint = daySetpoint;
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            if (statusText != null)
                statusText.text = $"HVAC: {currentSetpoint:F1}°C ({currentMode})";
        }

        protected override void ReceivedMessage(MmMessage message)
        {
            base.ReceivedMessage(message);

            switch (message.MmMethod)
            {
                case MmMethod.Switch:
                    if (message is MmMessageString switchMsg)
                    {
                        currentMode = switchMsg.value;
                        if (currentMode == "Night")
                        {
                            currentSetpoint = nightSetpoint;
                            isActive = false;  // Set but never checked!
                        }
                        else
                        {
                            currentSetpoint = daySetpoint;
                            isActive = true;
                        }
                        UpdateStatusText();
                        Debug.Log($"[HVAC] Mode changed to {currentMode}, setpoint: {currentSetpoint}°C");
                    }
                    break;

                case MmMethod.MessageFloat:
                    // BUG: This processes temperature adjustments regardless of mode
                    // It should only adjust during Day mode
                    if (message is MmMessageFloat tempMsg)
                    {
                        AdjustTemperature(tempMsg.value);
                    }
                    break;
            }
        }

        /// <summary>
        /// Adjusts the HVAC setpoint based on a temperature request.
        /// BUG: This runs even during Night mode!
        /// </summary>
        private void AdjustTemperature(float requestedTemp)
        {
            // BUG: Missing check for isActive or currentMode
            currentSetpoint = requestedTemp;
            UpdateStatusText();
            Debug.Log($"[HVAC] Adjusted setpoint to {currentSetpoint}°C (mode: {currentMode})");

            // Notify parent of the change
            var relay = GetComponent<MmRelayNode>();
            if (relay != null)
                relay.NotifyValue($"HVAC: {currentSetpoint}°C");
        }
    }
}
