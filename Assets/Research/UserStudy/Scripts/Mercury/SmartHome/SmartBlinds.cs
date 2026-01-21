using UnityEngine;
using MercuryMessaging;

namespace UserStudy.SmartHome.Mercury
{
    /// <summary>
    /// Smart blinds device that opens and closes based on commands.
    /// Responds to mode changes with different blind positions.
    /// </summary>
    public class SmartBlinds : MmBaseResponder
    {
        [SerializeField] private Transform blindsTransform;
        [SerializeField] private float closedPosition = 0f;
        [SerializeField] private float openPosition = 1f;
        [SerializeField] private float moveSpeed = 2f;

        private float targetPosition = 1f; // Start open
        private float currentPosition = 1f;

        public override void Awake()
        {
            base.Awake();

            // Set tag for filtering (Tag1 = Climate)
            Tag = MmTag.Tag1;
            TagCheckEnabled = true;
        }

        new void Update()
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

        /// <summary>
        /// Handles SetActive messages (open/close directly).
        /// </summary>
        public override void SetActive(bool active)
        {
            targetPosition = active ? 1f : 0f;

            // Report status
            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.MessageString,
                $"{gameObject.name}: {(active ? "Open" : "Closed")}",
                new MmMetadataBlock(MmLevelFilter.Parent)
            );
        }

        /// <summary>
        /// Handles Switch messages (mode changes).
        /// Modes: "Home", "Away", "Sleep"
        /// </summary>
        protected override void Switch(string modeName)
        {
            switch (modeName)
            {
                case "Home": // Home mode
                    targetPosition = 1f; // Open
                    break;

                case "Away": // Away mode
                    targetPosition = 0f; // Closed for security
                    break;

                case "Sleep": // Sleep mode
                    targetPosition = 0f; // Closed for darkness
                    break;
            }
        }
    }
}
