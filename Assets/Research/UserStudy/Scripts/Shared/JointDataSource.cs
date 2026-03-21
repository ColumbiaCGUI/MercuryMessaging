using UnityEngine;
using UnityEngine.Events;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Simulates a robot arm joint that produces changing angle data.
    /// Calls OnJointAngleChanged every frame with the current angle.
    /// DO NOT MODIFY THIS FILE.
    /// </summary>
    public class JointDataSource : MonoBehaviour
    {
        [Header("Joint Configuration")]
        public float minAngle = -90f;
        public float maxAngle = 90f;
        public float speed = 30f; // degrees per second

        [Header("Events")]
        public UnityEvent<float> OnJointAngleChanged;

        private float currentAngle;
        private float direction = 1f;

        void Update()
        {
            currentAngle += direction * speed * Time.deltaTime;
            if (currentAngle >= maxAngle || currentAngle <= minAngle)
                direction *= -1f;

            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
            OnJointAngleChanged?.Invoke(currentAngle);
        }

        public float GetCurrentAngle() => currentAngle;
    }
}
