using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Training practice: Events proximity filtering sender.
    /// Participants fill in the TODO section to iterate over receivers
    /// and call OnPing on those within range using Vector3.Distance.
    /// </summary>
    public class ProximityPracticeSender : MonoBehaviour
    {
        [SerializeField] private ProximityPracticeReceiver[] receivers;

        [Header("Configuration")]
        [Tooltip("Radius for proximity filtering (meters)")]
        public float radius = 4f;

        void Update()
        {
            // ============================================
            // YOUR CODE HERE
            // Loop through each receiver in the receivers array.
            // For each one, calculate the distance from this object
            // to the receiver using Vector3.Distance.
            // If the distance is within the radius, call:
            //   receiver.OnPing("ping");
            //
            // Example:
            //   foreach (var receiver in receivers)
            //   {
            //       float dist = Vector3.Distance(
            //           transform.position,
            //           receiver.transform.position
            //       );
            //       if (dist <= radius)
            //           receiver.OnPing("ping");
            //   }
            // ============================================
        }
    }
}
