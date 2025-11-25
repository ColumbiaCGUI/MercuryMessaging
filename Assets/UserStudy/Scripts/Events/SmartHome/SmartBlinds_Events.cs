using UnityEngine;

namespace UserStudy.SmartHome.Events
{
    /// <summary>
    /// Smart blinds device using Unity Events approach.
    /// Contrast with Mercury: Requires explicit controller reference.
    /// </summary>
    public class SmartBlinds_Events : MonoBehaviour, ISmartDevice
    {
        [Header("Controller Reference - Must be manually assigned")]
        [SerializeField] private SmartHomeController controller;

        [Header("Blinds Settings")]
        [SerializeField] private Transform blindsTransform;
        [SerializeField] private float closedPosition = 0f;
        [SerializeField] private float openPosition = 1f;
        [SerializeField] private float moveSpeed = 2f;

        private float targetPosition = 1f; // Start open
        private float currentPosition = 1f;

        #region ISmartDevice Implementation
        public GameObject GameObject => gameObject;
        public string DeviceName => gameObject.name;
        public DeviceType DeviceType => DeviceType.Climate;

        public void SetDeviceActive(bool active)
        {
            targetPosition = active ? 1f : 0f;

            // Report status to controller (tight coupling)
            if (controller != null)
            {
                controller.OnDeviceStatusChanged(DeviceName, active ? "Open" : "Closed");
            }
        }

        public void SetMode(string modeName)
        {
            switch (modeName)
            {
                case "Home":
                    targetPosition = 1f; // Open
                    break;

                case "Away":
                    targetPosition = 0f; // Closed for security
                    break;

                case "Sleep":
                    targetPosition = 0f; // Closed for darkness
                    break;
            }
        }
        #endregion

        void Update()
        {
            // Smoothly animate blind position
            if (Mathf.Abs(currentPosition - targetPosition) > 0.01f)
            {
                currentPosition = Mathf.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);

                if (blindsTransform != null)
                {
                    // Animate blinds (scale Y or position Y)
                    Vector3 scale = blindsTransform.localScale;
                    scale.y = Mathf.Lerp(closedPosition, openPosition, currentPosition);
                    blindsTransform.localScale = scale;
                }
            }
        }
    }
}
