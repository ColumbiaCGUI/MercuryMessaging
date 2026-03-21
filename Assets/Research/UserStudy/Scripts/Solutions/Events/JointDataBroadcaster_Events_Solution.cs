using UnityEngine;
using TMPro;

namespace MercuryMessaging.Research.UserStudy
{
    public class JointDataBroadcaster_Events_Solution : MonoBehaviour
    {
        [SerializeField] private JointAngleDisplay_Events panel1;
        [SerializeField] private JointAngleDisplay_Events panel2;
        [SerializeField] private JointAngleDisplay_Events panel3;
        [SerializeField] private JointAngleDisplay_Events panel4;

        public void SendJointData(float angle)
        {
            if (panel1 != null) panel1.UpdateAngle(angle);
            if (panel2 != null) panel2.UpdateAngle(angle);
            if (panel3 != null) panel3.UpdateAngle(angle);
            if (panel4 != null) panel4.UpdateAngle(angle);
        }
    }
}
