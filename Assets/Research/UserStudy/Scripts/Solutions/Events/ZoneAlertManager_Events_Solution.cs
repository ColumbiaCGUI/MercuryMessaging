using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    public class ZoneAlertManager_Events_Solution : MonoBehaviour
    {
        public Transform workerTransform;

        [SerializeField] private SafetyZoneIndicator_Events indicator1;
        [SerializeField] private SafetyZoneIndicator_Events indicator2;
        [SerializeField] private SafetyZoneIndicator_Events indicator3;
        [SerializeField] private SafetyZoneIndicator_Events indicator4;

        void Update()
        {
            if (workerTransform == null) return;

            CheckAndAlert(indicator1);
            CheckAndAlert(indicator2);
            CheckAndAlert(indicator3);
            CheckAndAlert(indicator4);
        }

        private void CheckAndAlert(SafetyZoneIndicator_Events indicator)
        {
            if (indicator == null) return;
            float dist = Vector3.Distance(workerTransform.position, indicator.transform.position);

            if (dist <= 1f)
                indicator.HandleAlert("emergency");
            else if (dist <= 2f)
                indicator.HandleAlert("warning");
            else
                indicator.HandleAlert("clear");
        }
    }
}
