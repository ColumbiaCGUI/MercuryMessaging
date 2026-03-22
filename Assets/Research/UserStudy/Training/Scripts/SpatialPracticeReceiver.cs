using UnityEngine;
using MercuryMessaging;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Training practice: Mercury spatial filtering receiver.
    /// Logs receipt of messages with the GameObject's name.
    /// Used in the spatial filtering practice exercise during training.
    /// </summary>
    public class SpatialPracticeReceiver : MmBaseResponder
    {
        protected override void ReceivedMessage(MmMessage message)
        {
            base.ReceivedMessage(message);
            if (message is MmMessageString strMsg)
            {
                Debug.Log($"[{gameObject.name}] Received: {strMsg.value}");
            }
        }
    }
}
