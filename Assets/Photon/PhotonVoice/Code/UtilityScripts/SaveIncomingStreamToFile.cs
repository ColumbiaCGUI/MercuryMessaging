﻿namespace Photon.Voice.Unity.UtilityScripts
{
    using UnityEngine;
    using System.IO;

    [RequireComponent(typeof(VoiceConnection))]
    [DisallowMultipleComponent]
    public class SaveIncomingStreamToFile : VoiceComponent
    {
        private VoiceConnection voiceConnection;

        [SerializeField]
        private bool muteLocalSpeaker = false;

        protected override void Awake()
        {
            base.Awake();
            this.voiceConnection = this.GetComponent<VoiceConnection>();
            this.voiceConnection.RemoteVoiceAdded += this.OnRemoteVoiceAdded;
            this.voiceConnection.SpeakerLinked += this.OnSpeakerLinked;
        }

        private void OnSpeakerLinked(Speaker speaker)
        {
            if (this.muteLocalSpeaker && speaker.RemoteVoice.PlayerId == voiceConnection.Client.LocalPlayer.ActorNumber)
            {
                AudioSource audioSource = speaker.GetComponent<AudioSource>();
                audioSource.mute = true;
                audioSource.volume = 0f;
            }
        }

        private void OnDestroy()
        {
            this.voiceConnection.RemoteVoiceAdded -= this.OnRemoteVoiceAdded;
        }

        private void OnRemoteVoiceAdded(RemoteVoiceLink remoteVoiceLink)
        {
            int bitsPerSample = 32;
            string filePath = this.GetFilePath(remoteVoiceLink);
            this.Logger.Log(LogLevel.Info, "Incoming stream {0}, output file path: {1}", remoteVoiceLink.VoiceInfo, filePath);
            WaveWriter waveWriter = new WaveWriter(filePath, remoteVoiceLink.VoiceInfo.SamplingRate, bitsPerSample, remoteVoiceLink.VoiceInfo.Channels);
            remoteVoiceLink.FloatFrameDecoded += f => { waveWriter.WriteSamples(f.Buf, 0, f.Buf.Length); };
            remoteVoiceLink.RemoteVoiceRemoved += () =>
            {
                this.Logger.Log(LogLevel.Info, "Remote voice stream removed: Saving wav file.");
                waveWriter.Dispose();
            };
        }

        private string GetFilePath(RemoteVoiceLink remoteVoiceLink)
        {
            string filename = string.Format("in_{0}_{1}_{2}_{3}_{4}.wav",
                System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss-ffff"), Random.Range(0, 1000),
                remoteVoiceLink.ChannelId, remoteVoiceLink.PlayerId, remoteVoiceLink.VoiceId);
            return Path.Combine(Application.persistentDataPath, filename);
        }
    }
}

