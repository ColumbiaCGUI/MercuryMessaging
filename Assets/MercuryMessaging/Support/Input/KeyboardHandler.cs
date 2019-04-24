// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met: 
// 
//  * Redistributions of source code must retain the above copyright notice, 
//    this list of conditions and the following disclaimer.
//  * Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//  * Neither the name of Columbia University nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE. 
//  
// =============================================================
// Authors: 
// Carmine Elvezio, Mengu Sukan, Steven Feiner
// =============================================================
//  
//  
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MercuryMessaging.Support.GUI;
using UnityEngine;
using UnityEngine.Events;

namespace MercuryMessaging.Support.Input
{
    /// <summary>
    /// Sample Keyboard Handler, useful for creating simple
    /// input handling using MercuryMessaging.
    /// </summary>
    class KeyboardHandler : MonoBehaviour
    {
        public static int Delay = 120; // Frames

        Modifier mods;

        [System.Flags]
        public enum Modifier
        {
            None = 0,
            Alt = 1,
            Ctrl = 2,
            Shift = 4
        };

        public enum RespondWhen
        {
            Pressed,
            Clutched,
            Released
        }
	
        internal class Entry
        {
            public KeyCode Key;
            public string Description;
            public UnityAction Response;
            public Modifier Modifier;
            public RespondWhen ResponseTiming;
            public int CountDown;
            public UnityAction ClutchAbortResponse;
            public UnityAction ClutchReleaseResponse;
            public bool IsAllowed;
		
            public override string ToString()
            {
                var sb = new StringBuilder();
                if (Modifier != Modifier.None) sb.Append(Modifier + "+");
                sb.Append(Key);
                if (ResponseTiming == RespondWhen.Released) sb.Append(" (Release)");
                if (ResponseTiming == RespondWhen.Clutched) sb.Append(" (Clutch)");
                sb.Append(" - ");
                sb.Append(Description);
                return sb.ToString();
            }
        }
	
        public static readonly Dictionary<KeyCode, Entry> Entries = new Dictionary<KeyCode, Entry>();
        public static readonly Dictionary<KeyCode, KeyCode> EquivalentKeyCode = new Dictionary<KeyCode, KeyCode>();
        private static readonly List<Entry> DelayedEntries = new List<Entry>();
        public static MmLogger.LogFunc LogMessage;
	
        public static void HandleKeyPress(KeyCode key)
        {
            HandleKeyPress(key, Modifier.None);
        }
	
        public static void HandleKeyRelease(KeyCode key)
        {
            HandleKeyRelease(key, Modifier.None);
        }
	
        public static void HandleKeyPress(KeyCode key, Modifier modifier)
        {
            Log(string.Format("Raw,Press,{0}", key));
		
            KeyCode equivalentKey;
            if (EquivalentKeyCode.TryGetValue(key, out equivalentKey)) {
                HandleKeyPress(equivalentKey, modifier);
                return;
            }
		
            Entry e;
		
            // No entry
            if (!Entries.TryGetValue(key, out e)) return;
            // Not allowed
            if (!e.IsAllowed) return;
            // Shift modifier doesn't match
            if (modifier != e.Modifier) return;
		
            switch (e.ResponseTiming)
            {
                case RespondWhen.Released:
                    // This is an entry that should trigger on release, not press
                    return;
                case RespondWhen.Clutched:
                    // This is a delayed keypress, initialize countdown
                    e.CountDown = Delay;
                    return;
                default:
                    // All checks passed, respond
                    Log(string.Format("Cmd,Press,{0},{1}", key, e.Description));
                    Call(e.Response);
                    break;
            }
        }

        public static void HandleKeyRelease(KeyCode key, Modifier modifier)
        {
            Log(string.Format("Raw,Release,{0}", key));
		
            KeyCode equivalentKey;
            if (EquivalentKeyCode.TryGetValue(key, out equivalentKey))
                HandleKeyRelease(equivalentKey, modifier);
		
            Entry e;
		
            // No entry
            if (!Entries.TryGetValue(key, out e)) return;
            // Not allowed
            if (!e.IsAllowed) return;
            // Shift modifier doesn't match
            if (modifier != e.Modifier) return;
		
            switch (e.ResponseTiming)
            {
                case RespondWhen.Clutched:
                    // This is a delayed keypress
                    // CountDown isn't finished, we're aborting
                    if (e.CountDown > 0) {
                        // Reset countdown
                        e.CountDown = 0;
                        // Respond to abort
                        Log(string.Format("Cmd,Release,{0},{1},ClutchAbort", key, e.Description));
                        Call(e.ClutchAbortResponse);
                    }
                    else
                    {
                        // Countdown already finished, we can respond to release
                        if (e.ClutchReleaseResponse != null)
                            Log(string.Format("Cmd,Release,{0},{1},ClutchDone", key, e.Description));
                        Call(e.ClutchReleaseResponse);
                    }
                    break;
                case RespondWhen.Pressed:
                    // This is an entry that should trigger on press, not release
                    return;
                default:
                    // All checks passed, respond
                    Log(string.Format("Cmd,Release,{0},{1},Release", key, e.Description));
                    Call(e.Response);
                    break;
            }
        }
	
        public static string Descriptions()
        {
            var i = 1;
            return Entries.Aggregate("", (current, entry) => current + (string.Format("\n({0}) {1}\n", i++, entry.Value.ToString())));
        }

        // TODO: Make this capable of handling multiple callbacks per key
        public static void AddEntry(KeyCode key, string description, UnityAction response,
            RespondWhen respondWhen = RespondWhen.Pressed,
            Modifier modifier = Modifier.None,
            UnityAction clutchAbortResponse = null,
            UnityAction clutchReleaseResponse = null)
        {
            var e = new Entry
            {
                Key = key,
                Description = description,
                Response = response,
                Modifier = modifier,
                ResponseTiming = respondWhen,
                ClutchAbortResponse = clutchAbortResponse,
                ClutchReleaseResponse = clutchReleaseResponse,
                IsAllowed = true
            };
            Entries[key] = e;
            if (respondWhen == RespondWhen.Clutched) DelayedEntries.Add(e);
        }
	
        public static void ProcessDelayedEntries()
        {
            foreach (var de in DelayedEntries.Where(e => e.CountDown > 0))
            {
                de.CountDown--;
                //Logger.Log.Debug(delayedEntry.CountDown);
                if (de.CountDown != 0) continue;
			
                Log(string.Format("Cmd,Delay,{0},{1}", de.Key, de.Description));
			
                if (de.IsAllowed)
                    Call(de.Response);
            }
        }
	
        public static void Call(UnityAction a)
        {
            if (a != null) a();
        }

        public static void Log(string msg) {
            if (LogMessage != null) LogMessage(msg);
        }

        public void Update() {

            mods = Modifier.None;

            if (UnityEngine.Input.GetKey(KeyCode.LeftAlt) || UnityEngine.Input.GetKey(KeyCode.RightAlt))
                mods |= Modifier.Alt;

            if (UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl))
                mods |= Modifier.Ctrl;

            if (UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift))
                mods |= Modifier.Shift;

            foreach (var e in Entries)
            {
                if(UnityEngine.Input.GetKeyDown(e.Key))
                    HandleKeyPress(e.Key, mods);
                if(UnityEngine.Input.GetKeyUp(e.Key))
                    HandleKeyRelease(e.Key, mods);
            }

            foreach (var e in EquivalentKeyCode)
            {
                if(UnityEngine.Input.GetKeyDown(e.Value))
                    HandleKeyPress(e.Key, mods);
                if(UnityEngine.Input.GetKeyUp(e.Value))
                    HandleKeyRelease(e.Key, mods);
            }

            ProcessDelayedEntries();

            //GUILayout.Label("Press Enter To Start Game");
        }

        public void Start()
        {
            LogMessage += MmLogger.LogApplication;

            AddEntry(KeyCode.F1, "Toggle Help Menu", delegate
            {
                MmGuiHandler.Instance.SetFullScreenText("<b><size=20>List of Available Commands</size></b>\n" + Descriptions());
                MmGuiHandler.Instance.ToggleFullScreenText();
            });

            MmGuiHandler.Instance.ShowMessage("Press F1 for help menu",1);
        
            /* Test Calls
        AddEntry(KeyCode.M, "Test Message",
                                 delegate {
                                     ShowMessage("This is a test");
                                 }
        );

        AddEntry(KeyCode.H, "KeyPress Test",
                                 delegate { 
                                    Debug.Log ("Inside Delegate");
                                 }
        );

        AddEntry(KeyCode.C, "Clutch Test",
                                 delegate {
                                    Debug.Log ("Inside Clutch Start");
                                 },
                                 RespondWhen.Clutched,
                                 clutchAbortResponse: delegate {
                                    Debug.Log ("Inside Clutch Abort");
                                 },
                                 clutchReleaseResponse: delegate {
                                    Debug.Log ("Inside Clutch Release");
                                 }
        );

        AddEntry(KeyCode.E, "Modifier Test",
                                 delegate {
                                    Debug.Log ("Inside Modifier Test");
                                 },
                                 RespondWhen.Released,
                                 Modifier.Shift
        );
        */
        }
    }
}