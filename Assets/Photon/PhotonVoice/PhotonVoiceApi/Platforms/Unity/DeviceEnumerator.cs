using UnityEngine;
using System.Collections.Generic;

namespace Photon.Voice.Unity
{
    public class AudioInEnumerator : DeviceEnumeratorBase
    {
        public AudioInEnumerator(ILogger logger) : base(logger)
        {
            Refresh();
        }

        public override void Refresh()
        {
            var unityDevs = UnityMicrophone.devices;
            devices = new List<DeviceInfo>();
            for (int i = 0; i < unityDevs.Length; i++)
            {
                var d = unityDevs[i];
                devices.Add(new DeviceInfo(d));
            }

            if (OnReady != null)
            {
                OnReady();
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        public override bool IsSupported => false;

        public override string Error { get { return "Current platform " + Application.platform + " is not supported by AudioInEnumerator."; } }
#else
        public override string Error { get { return null; } }
#endif

        public override void Dispose()
        {
        }
    }

#if PHOTON_VOICE_VIDEO_ENABLE
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
    public class VideoInEnumerator : DeviceEnumeratorBase
    {
        public VideoInEnumerator(ILogger logger) : base(logger)
        {
            Refresh();
        }

        public override void Refresh()
        {
            var unityDevs = UnityEngine.WebCamTexture.devices;
            devices = new List<DeviceInfo>();
            for (int i = 0; i < unityDevs.Length; i++)
            {
                var d = unityDevs[i];
                devices.Add(new DeviceInfo(d.name));
            }

            if (OnReady != null)
            {
                OnReady();
            }
        }

        public override string Error { get { return null; } }

        public override void Dispose()
        {
        }
    }
#endif
#endif
}