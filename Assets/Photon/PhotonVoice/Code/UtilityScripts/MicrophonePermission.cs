﻿#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

#if (UNITY_IOS || UNITY_VISIONOS) && !UNITY_EDITOR
using System.Collections;
using UnityEngine.iOS;
#endif

using System;
using UnityEngine;

namespace Photon.Voice.Unity.UtilityScripts
{
    /// <summary>
    /// Helper to request Microphone permission on Android or iOS.
    /// </summary>
    public class MicrophonePermission : VoiceComponent
    {
#if (UNITY_ANDROID || (UNITY_IOS || UNITY_VISIONOS)) && !UNITY_EDITOR
        private bool isRequesting;
#endif
        private bool hasPermission;

        public static event Action<bool> MicrophonePermissionCallback;

        [SerializeField]
        private bool autoStart = true;

        public bool HasPermission
        {
            get
            {
                return this.hasPermission;
            }
            private set
            {
                this.Logger.Log(LogLevel.Info, "Microphone Permission Granted: {0}", value);
                MicrophonePermissionCallback?.Invoke(value);
                if (this.hasPermission != value)
                {
                    this.hasPermission = value;
                    if (this.hasPermission && this.autoStart)
                    {
                        var recorder = this.GetComponent<Recorder>();
                        if (recorder != null)
                        {
                            if (!recorder.RecordingEnabled)
                            {
                                this.Logger.Log(LogLevel.Info, "Starting recording automatically");
                            }
                            recorder.RecordingEnabled = true;
                        }
                        else
                        {
                            this.Logger.Log(LogLevel.Info, "Recorder not found. Assign MicrophonePermission to an object with Recorder to automatically start recording");
                        }
                    }
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            this.InitVoice();
        }

#if (UNITY_IOS || UNITY_VISIONOS) && !UNITY_EDITOR
        IEnumerator PermissionCheck()
        {
            this.isRequesting = true;
            this.Logger.Log(LogLevel.Info, "iOS Microphone Permission Request");
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            this.isRequesting = false;
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                this.HasPermission = true;
            }
            else
            {
                this.HasPermission = false;
            }
        }
#endif

        public void InitVoice()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                this.HasPermission = true;
            }
            else
            {
                this.Logger.Log(LogLevel.Info, "Android Microphone Permission Request");
#if UNITY_2020_2_OR_NEWER
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
                callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
                callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
                Permission.RequestUserPermission(Permission.Microphone, callbacks);
#else
                Permission.RequestUserPermission(Permission.Microphone);
#endif
                this.isRequesting = true;
            }
#elif (UNITY_IOS || UNITY_VISIONOS) && !UNITY_EDITOR
            this.StartCoroutine(this.PermissionCheck());
#else
            this.HasPermission = true;
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
#if UNITY_2020_2_OR_NEWER
        internal void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
        {
            this.isRequesting = false;
            this.HasPermission = false;
            this.Logger.Log(LogLevel.Info, $"{permissionName} PermissionDeniedAndDontAskAgain");
        }

        internal void PermissionCallbacks_PermissionGranted(string permissionName)
        {
            this.isRequesting = false;
            this.HasPermission = true;
            this.Logger.Log(LogLevel.Info, $"{permissionName} PermissionGranted");
        }

        internal void PermissionCallbacks_PermissionDenied(string permissionName)
        {
            this.isRequesting = false;
            this.HasPermission = false;
            this.Logger.Log(LogLevel.Info, $"{permissionName} PermissionDenied");
        }
#else
        private void OnApplicationFocus(bool focus)
        {
            if (focus && this.isRequesting)
            {
                if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
                {
                    this.HasPermission = true;
                }
                else
                {
                    this.HasPermission = false;
                }
                this.isRequesting = false;
            }
        }
#endif
#endif
    }
}