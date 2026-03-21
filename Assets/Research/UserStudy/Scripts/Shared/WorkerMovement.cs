using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Allows manual movement of the worker avatar for testing safety zones.
    /// Uses arrow keys or WASD to move on the XZ plane.
    /// DO NOT MODIFY THIS FILE.
    /// </summary>
    public class WorkerMovement : MonoBehaviour
    {
        public float moveSpeed = 2f;

        void Update()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            transform.Translate(new Vector3(h, 0, v) * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}
