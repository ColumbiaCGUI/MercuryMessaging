using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Training practice: Events proximity filtering receiver.
    /// Plain MonoBehaviour with an OnPing method that logs receipt.
    /// Used in the proximity filtering practice exercise during training.
    /// </summary>
    public class ProximityPracticeReceiver : MonoBehaviour
    {
        public void OnPing(string message)
        {
            Debug.Log($"[{gameObject.name}] Received: {message}");
        }
    }
}
