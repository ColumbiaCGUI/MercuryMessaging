using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// T1: Sensor Fan-Out — SOLUTION
    /// Broadcasts joint angle data to all child UI panels using Mercury.
    /// </summary>
    public class JointDataBroadcaster_Solution : MonoBehaviour
    {
        private MmRelayNode relay;

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
        }

        /// <summary>
        /// Called every frame by JointDataSource with the current angle.
        /// Sends the angle to all child UI panels via Mercury broadcast.
        /// </summary>
        public void SendJointData(float angle)
        {
            relay.BroadcastValue(angle);
        }
    }
}
