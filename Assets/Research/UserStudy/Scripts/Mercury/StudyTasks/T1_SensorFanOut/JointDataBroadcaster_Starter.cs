using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// T1: Sensor Fan-Out — Broadcast joint angle data to all UI panels.
    /// TASK: Implement SendJointData() to send the joint angle to all 4 UI panels
    /// using Mercury's messaging API.
    ///
    /// HINT: The UI panels are children of this relay node in the hierarchy.
    /// You need ONE line of Mercury code.
    /// </summary>
    public class JointDataBroadcaster_Starter : MonoBehaviour
    {
        private MmRelayNode relay;

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
        }

        /// <summary>
        /// Called every frame by JointDataSource with the current angle.
        /// TODO: Send the angle to all child UI panels using Mercury.
        /// </summary>
        public void SendJointData(float angle)
        {
            // ============================================
            // YOUR CODE HERE (1 line of Mercury code)
            // Send the angle value to all child UI panels.
            // ============================================
        }
    }
}
