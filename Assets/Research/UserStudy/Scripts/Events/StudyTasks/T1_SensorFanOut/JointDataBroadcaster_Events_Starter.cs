using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// T1: Sensor Fan-Out — Broadcast joint angle data to all UI panels.
    /// TASK: Send joint angle data to all 4 UI panel displays.
    ///
    /// You have two approaches:
    ///   Option A: Use UnityEvent<float> and wire each panel in the Inspector
    ///   Option B: Use [SerializeField] references to each panel and call methods directly
    ///
    /// Either approach is acceptable. You need to:
    /// 1. Declare your event/reference fields
    /// 2. Implement SendJointData to update all 4 panels
    /// </summary>
    public class JointDataBroadcaster_Events_Starter : MonoBehaviour
    {
        // ============================================
        // YOUR CODE HERE
        // Declare references or events for 4 UI panels.
        // Each panel has a JointAngleDisplay_Events component
        // with an UpdateAngle(float) method.
        // ============================================

        /// <summary>
        /// Called every frame by JointDataSource with the current angle.
        /// TODO: Send the angle to all 4 UI panels.
        /// </summary>
        public void SendJointData(float angle)
        {
            // ============================================
            // YOUR CODE HERE
            // Send the angle to each panel.
            // ============================================
        }
    }
}
