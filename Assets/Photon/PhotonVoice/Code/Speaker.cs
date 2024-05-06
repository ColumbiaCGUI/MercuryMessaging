// ----------------------------------------------------------------------------
// <copyright file="Speaker.cs" company="Exit Games GmbH">
//   Photon Voice for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
// Component representing remote audio stream in local scene.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Photon.Voice.Unity
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Photon Voice/Speaker")]
    [DisallowMultipleComponent]
    public class Speaker : VoiceComponent
    {
        #region Private Fields

        protected IAudioOut<float> audioOutput;

#if UNITY_WEBGL && UNITY_2021_2_OR_NEWER && !UNITY_EDITOR // requires ES6, allows non-WebGL workflow in Editor
        // apply AudioSource parameters, Speaker position and AudioListener transform to WebAudioAudioOut
        private WebAudioAudioOut webOut;           // not null if WebAudio is supported
        private AudioSource webOutAudioSource;     // not null if WebAudio is supported and AudioSource exists, we apply its volume and spatial blend to WebAudioAudioOut
        private Transform webOutListenerTransform; // not null if WebAudio is supported, AudioListener exists and initial spatialBlend > 0 (3D enabled)
#endif

        [SerializeField]
        protected AudioOutDelayControl.PlayDelayConfig playDelayConfig = AudioOutDelayControl.PlayDelayConfig.Default;

        [SerializeField]
        protected bool restartOnDeviceChange = true;

#endregion

#region Public Fields

#if UNITY_PS4 || UNITY_PS5
        /// <summary>Set the PlayStation User ID to determine on which user's headphones to play audio.</summary>
        /// <remarks>
        /// Note: at the moment, only the first Speaker can successfully set the User ID.
        /// Subsequently initialized Speakers will play their audio on the headphones that have been set with the first Speaker initialized.
        public int PlayStationUserID = 0;

        public enum AudioOutputPlugin
        {
            /// <summary>Route audio output directly to the Sony audio output APIs, without going through Unity
            /// <remarks>This is simple to use (no mixer setup required), but does not support the use of an Unity AudioMixer for Voice Chat output.
            PhotonVoiceAudioOutputPlugin,
            /// <summary>Send audio output to Unity and afterwards re-route the output of the Unity audio mixer to the Sony audio output APIs.
            // <remarks>This enables support for using Unity AudioMixer on PlayStation, but requires the app to
            // <remarks>- specify an AudioMixer as the output for the AudioSource component of your Photon Voice Speaker prefab
           // <remarks>- to add the 'RouteOutputToSonyPSNativeAPI' effect from AudioPluginPhotonVoice to that AudioMixer
            AudioPluginPhotonVoice,
            /// <summary>The default value is PhotonVoiceAudioOutputPlugin
            Default = PhotonVoiceAudioOutputPlugin
        }

        public AudioOutputPlugin OutputPlugin;
#endif

#endregion

#region Properties

        /// <summary>Is the speaker playing right now.</summary>
        public bool IsPlaying
        {
            get { return audioOutput != null && this.audioOutput.IsPlaying; }
        }

        /// <summary>The current difference between positions in the buffer of (jittery) stream writer and (clock-driven) audio output reader in ms.</summary>
        public int Lag
        {
            get { return this.audioOutput == null ? 0 : this.audioOutput.Lag; }
        }

        /// <summary>
        /// Register a method to be called when remote voice removed.
        /// </summary>
        public Action<Speaker> OnRemoteVoiceRemoveAction { get; set; }

        public RemoteVoiceLink RemoteVoice { get; private set; }

        /// <summary>
        /// Whether or not this Speaker has been linked to a remote voice stream.
        /// </summary>
        public bool IsLinked
        {
            get { return this.RemoteVoice != null; }
        }

        /// <summary>Gets or sets jitter buffer config.</summary>
        /// <remarks>
        /// Make sure that the new value is fully initialized or built from <see cref="AudioOutDelayControl.PlayDelayConfig.Default"></see>.
        /// </remarks>
        public AudioOutDelayControl.PlayDelayConfig PlayDelayConfig
        {
            get => this.playDelayConfig;
            set
            {
                if (this.playDelayConfig.Low != value.Low || this.playDelayConfig.High != value.High || this.playDelayConfig.Max != value.Max)
                {
                    this.playDelayConfig = value;
                    this.RestartPlayback();
                }
            }
        }

        /// <summary>Gets or sets jitter buffer size in ms.</summary>
        /// <remarks>
        /// The method updates PlayDelayConfig with reasonable values based on the single value provided.
        /// Use <see cref="PlayDelayConfig"></see> for more precise control.
        /// </remarks>
        public int PlayDelay
        {
            get => this.playDelayConfig.Low;
            set
            {
                var l = value;
                var h = value; // rely on automatic tolerance value
                var m = 1000; // as in PlayDelayConfig.Default
                if (this.playDelayConfig.Low != l || this.playDelayConfig.High != h || this.playDelayConfig.Max != m)
                {
                    this.playDelayConfig.Low = l;
                    this.playDelayConfig.High = h;
                    this.playDelayConfig.Max = m;
                    this.RestartPlayback();
                }
            }
        }

#endregion

#region Private Methods

        protected override void Awake()
        {
            base.Awake();
            // update AudioSettings.OnAudioConfigurationChanged
            RestartOnDeviceChange = restartOnDeviceChange;
        }

        private void AudioConfigurationChangeHandler(bool deviceWasChanged)
        {
            this.Logger.Log(LogLevel.Info, "Audio configuration changed. Restarting.");
            RestartPlayback();
        }

        // called from Link() and when restarting
        private void Initialize()
        {
            this.Logger.Log(LogLevel.Info, "Initializing.");
            this.audioOutput = CreateAudioOut();
            this.Logger.Log(LogLevel.Info, "Initialized.");
        }

        protected virtual IAudioOut<float> CreateAudioOut()
        {
#if !UNITY_EDITOR && (UNITY_PS4 || UNITY_PS5)
            this.Logger.Log(LogLevel.Info, "OutputPlugin is set to " + OutputPlugin);
            if(OutputPlugin == AudioOutputPlugin.PhotonVoiceAudioOutputPlugin)
            {
                this.Logger.Log(LogLevel.Info, "sending output to PlayStationAudioOut.");
                return new Photon.Voice.PlayStation.PlayStationAudioOut(this.PlayStationUserID);
            }
            else
            {
                this.Logger.Log(LogLevel.Info, "sending output to Mixer.");
                this.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.SetFloat("PSUserID", this.PlayStationUserID);
                // fall through to the default return line at the end of this function
            }
#elif UNITY_WEBGL && !UNITY_EDITOR // allows non-WebGL workflow in Editor
    #if UNITY_2021_2_OR_NEWER // requires ES6
            webOutAudioSource = this.GetComponent<AudioSource>();
            double initSpatialBlend = webOutAudioSource != null ? webOutAudioSource.spatialBlend : 0;
            webOut = new WebAudioAudioOut(this.playDelayConfig, initSpatialBlend, this.Logger, string.Empty, true);
            if (initSpatialBlend > 0)
            {
                var al = FindObjectOfType<AudioListener>();
                if (al != null)
                {
                    webOutListenerTransform = al.gameObject.transform;
                }
                else
                {
                    webOutListenerTransform = null;
                }
            }

            return webOut;
    #else
            this.Logger.Log(LogLevel.Error, "Speaker requies Unity 2021.2 or newer for WebGL");
            return new AudioOutDummy<float>();
    #endif
#endif
            return new UnityAudioOut(this.GetComponent<AudioSource>(), this.playDelayConfig, this.Logger, string.Empty, true);
        }

        internal bool Link(RemoteVoiceLink stream)
        {
            if (this.IsLinked)
            {
                this.Logger.Log(LogLevel.Warning, "Speaker already linked to {0}, cancelled linking to {1}", this.RemoteVoice, stream);
                return false;
            }
            if (stream.VoiceInfo.Channels <= 0) // early avoid possible crash due to ArgumentException in AudioClip.Create inside UnityAudioOut.Start
            {
                this.Logger.Log(LogLevel.Error, "Received voice info channels is not expected (<= 0), cancelled linking to {0}", stream);
                return false;
            }
            this.Logger.Log(LogLevel.Info, "Link {0}", stream);
            stream.RemoteVoiceRemoved += OnRemoteVoiceRemove;
            stream.FloatFrameDecoded += this.OnAudioFrame;
            this.RemoteVoice = stream;
            this.Initialize();           // new audioOutput is created
            return this.StartPlayback(); // starting audioOutput
        }

        private void OnRemoteVoiceRemove()
        {
            this.Logger.Log(LogLevel.Info, "OnRemoteVoiceRemove {0}", this.RemoteVoice);
            this.StopPlayback();
            if (this.OnRemoteVoiceRemoveAction != null) { this.OnRemoteVoiceRemoveAction(this); }
            this.Unlink();
        }

        private void OnAudioFrame(FrameOut<float> frame)
        {
            if (this.audioOutput != null)
            {
                this.audioOutput.Push(frame.Buf);
                if (frame.EndOfStream)
                {
                    this.audioOutput.Flush();
                }
            }
        }

        private bool StartPlayback()
        {
            if (this.RemoteVoice == null)
            {
                this.Logger.Log(LogLevel.Warning, "Cannot start playback because speaker is not linked");
                return false;
            }
            if (audioOutput == null)
            {
                this.Logger.Log(LogLevel.Warning, "Cannot start playback because not initialized yet");
                return false;
            }
            var vi = this.RemoteVoice.VoiceInfo;
            this.audioOutput.Start(vi.SamplingRate, vi.Channels, vi.FrameDurationSamples);
            this.Logger.Log(LogLevel.Info, "Speaker started playback: {0}, delay {1}", vi, this.playDelayConfig);
            return true;
        }

        protected virtual void OnDestroy()
        {
            this.Logger.Log(LogLevel.Info, "OnDestroy");
            this.StopPlayback();
            this.Unlink();
            AudioSettings.OnAudioConfigurationChanged -= AudioConfigurationChangeHandler;
        }

        // stopping audioOutput releases its resources
        private void StopPlayback()
        {
            this.Logger.Log(LogLevel.Info, "StopPlayback");
            if (this.audioOutput != null)
            {
                this.audioOutput.Stop();
                this.audioOutput = null;
            }
        }

        private void Unlink()
        {
            if (this.RemoteVoice != null)
            {
                this.RemoteVoice.FloatFrameDecoded -= this.OnAudioFrame;
                this.RemoteVoice.RemoteVoiceRemoved -= this.OnRemoteVoiceRemove;
                this.RemoteVoice = null;
            }
        }
        protected void Update()
        {
            if (System.Threading.Interlocked.Exchange(ref this.restartPlaybackPending, 0) != 0)
            {
                this.Logger.Log(LogLevel.Info, "Restarting playback");
                this.StopPlayback();  // stopping audioOutput releases its resources
                this.Initialize();    // new audioOutput is created
                this.StartPlayback(); // starting audioOutput
            }

            if (this.audioOutput != null)
            {
                this.audioOutput.Service();
            }

#if UNITY_WEBGL && UNITY_2021_2_OR_NEWER && !UNITY_EDITOR // requires ES6, allows non-WebGL workflow in Editor
            // if AudioSource is available, update audio node with its parameters
            if (webOutAudioSource != null)
            {
                webOut.SetVolume(webOutAudioSource.volume);

                // spatialBlend is needed only in 3D mode
                if (webOutListenerTransform != null)
                {
                    webOut.SetSpatialBlend(webOutAudioSource.spatialBlend);
                }
            }
            // update audio listener
            if (webOutListenerTransform != null)
            {
                var p = webOutListenerTransform.position;
                var f = webOutListenerTransform.forward;
                var u = webOutListenerTransform.up;
                // Unity is left-handed, y-up
                // WebAudio is right-hand, y-down
                webOut.SetListenerPosition(p.x, -p.y, p.z);
                webOut.SetListenerOrientation(f.x, -f.y, f.z, u.x, -u.y, u.z);

                // Speaker position
                p = gameObject.transform.position;
                webOut.SetPosition(p.x, p.y, p.z);
            }
#endif
        }

#endregion

#region Public Methods

        // prevents multiple restarts per Update()
        // int instead of bool to use Interlocked.Exchange()
        int restartPlaybackPending = 0;

        /// <summary>
        /// Restarts the audio playback of the linked incoming remote audio stream via AudioSource component.
        /// </summary>
        /// <returns>True if playback is successfully restarted.</returns>
        public void RestartPlayback()
        {
            restartPlaybackPending = 1;
        }

        public bool RestartOnDeviceChange
        {
            get => restartOnDeviceChange;
            set
            {
                restartOnDeviceChange = value;
                AudioSettings.OnAudioConfigurationChanged -= AudioConfigurationChangeHandler;
                if (restartOnDeviceChange)
                {
                    AudioSettings.OnAudioConfigurationChanged += AudioConfigurationChangeHandler;
                }
            }
        }

#endregion
    }
}


