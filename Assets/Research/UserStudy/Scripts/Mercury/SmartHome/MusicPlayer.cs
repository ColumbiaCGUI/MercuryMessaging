using UnityEngine;
using MercuryMessaging;

namespace UserStudy.SmartHome.Mercury
{
    /// <summary>
    /// Smart music player device with AudioSource integration.
    /// Plays/stops music based on commands and mode changes.
    /// </summary>
    public class MusicPlayer : MmBaseResponder
    {
        [SerializeField] private AudioSource audioSource;
        private bool isPlaying = false;

        public override void Awake()
        {
            base.Awake();

            // Set tag for filtering (Tag2 = Entertainment)
            Tag = MmTag.Tag2;
            TagCheckEnabled = true;

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        new void Update()
        {
            // Pulse visual feedback when music is playing
            if (isPlaying)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.15f;
                transform.localScale = new Vector3(0.6f, 0.8f, 0.4f) * pulse;
            }
            else
            {
                transform.localScale = new Vector3(0.6f, 0.8f, 0.4f); // Reset to original size
            }
        }

        /// <summary>
        /// Handles SetActive messages (play/stop commands).
        /// </summary>
        public override void SetActive(bool active)
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

        /// <summary>
        /// Handles Switch messages (mode changes).
        /// Modes: "Home", "Away", "Sleep"
        /// </summary>
        protected override void Switch(string modeName)
        {
            // Only Sleep mode affects music player
            if (modeName == "Sleep") // Sleep mode
            {
                Stop();
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

                // Report status
                GetComponent<MmRelayNode>().MmInvoke(
                    MmMethod.MessageString,
                    $"{gameObject.name}: Music playing",
                    new MmMetadataBlock(MmLevelFilter.Parent)
                );
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

                // Report status
                GetComponent<MmRelayNode>().MmInvoke(
                    MmMethod.MessageString,
                    $"{gameObject.name}: Music stopped",
                    new MmMetadataBlock(MmLevelFilter.Parent)
                );
            }
        }
    }
}
