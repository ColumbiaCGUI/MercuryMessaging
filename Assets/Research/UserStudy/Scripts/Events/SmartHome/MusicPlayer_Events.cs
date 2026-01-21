using UnityEngine;

namespace UserStudy.SmartHome.Events
{
    /// <summary>
    /// Smart music player device using Unity Events approach.
    /// Contrast with Mercury: Requires explicit controller reference.
    /// </summary>
    public class MusicPlayer_Events : MonoBehaviour, ISmartDevice
    {
        [Header("Controller Reference - Must be manually assigned")]
        [SerializeField] private SmartHomeController controller;

        [Header("Music Player Settings")]
        [SerializeField] private AudioSource audioSource;
        private bool isPlaying = false;

        #region ISmartDevice Implementation
        public GameObject GameObject => gameObject;
        public string DeviceName => gameObject.name;
        public DeviceType DeviceType => DeviceType.Entertainment;

        public void SetDeviceActive(bool active)
        {
            if (active)
            {
                Play();
            }
            else
            {
                Stop();
            }
        }

        public void SetMode(string modeName)
        {
            // Only Sleep mode affects music player
            if (modeName == "Sleep")
            {
                Stop();
            }
        }
        #endregion

        void Start()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        /// <summary>
        /// Starts playing music.
        /// </summary>
        private void Play()
        {
            if (!isPlaying && audioSource != null)
            {
                audioSource.Play();
                isPlaying = true;

                // Report status to controller (tight coupling)
                if (controller != null)
                {
                    controller.OnDeviceStatusChanged(DeviceName, "Music playing");
                }
            }
        }

        /// <summary>
        /// Stops playing music.
        /// </summary>
        private void Stop()
        {
            if (isPlaying && audioSource != null)
            {
                audioSource.Stop();
                isPlaying = false;

                // Report status to controller (tight coupling)
                if (controller != null)
                {
                    controller.OnDeviceStatusChanged(DeviceName, "Music stopped");
                }
            }
        }
    }
}
