using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// HVAC Controller — Solution for T3 Mode-Switch Debugging.
    ///
    /// FIX: AdjustTemperature now checks isActive before processing.
    /// When in Night mode, isActive is false and adjustment commands are ignored.
    /// </summary>
    public class HvacController_Solution : MmBaseResponder
    {
        [Header("HVAC Settings")]
        public float daySetpoint = 22f;
        public float nightSetpoint = 18f;

        private float currentSetpoint;
        private string currentMode = "Day";
        private bool isActive = true;

        void Start()
        {
            currentSetpoint = daySetpoint;
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
                            isActive = false;
                        }
                        else
                        {
                            currentSetpoint = daySetpoint;
                            isActive = true;
                        }
                        Debug.Log($"[HVAC] Mode changed to {currentMode}, setpoint: {currentSetpoint}°C");
                    }
                    break;

                case MmMethod.MessageFloat:
                    if (message is MmMessageFloat tempMsg)
                    {
                        AdjustTemperature(tempMsg.value);
                    }
                    break;
            }
        }

        private void AdjustTemperature(float requestedTemp)
        {
            if (!isActive) return;  // FIX: Only adjust during active (Day) mode
            currentSetpoint = requestedTemp;
            Debug.Log($"[HVAC] Adjusted setpoint to {currentSetpoint}°C (mode: {currentMode})");

            var relay = GetComponent<MmRelayNode>();
            if (relay != null)
                relay.NotifyValue($"HVAC: {currentSetpoint}°C");
        }
    }
}
