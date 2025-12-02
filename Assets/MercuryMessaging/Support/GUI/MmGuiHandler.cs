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
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MercuryMessaging.Support.GUI
{
	public class MmGuiHandler : MonoBehaviour, IMmGUI
    {

        #region Full Screen Text Properties

        string fullScreenText;

        #endregion

        #region Message Properties

        public float MessageDisplayTime = 2;
        public float MessageFadeTime = 0.5f;

        IEnumerator showingMessage;
        string message;
        float msgStartingAlpha;

        #endregion

        public GameObject Canvas;

        public Text FullscreenText;
        public GameObject FullscreenPanel;

        public Text TopRightText;
        public GameObject TopRightPanel;

        // Static singleton property
        public static MmGuiHandler Instance { get; private set; }

        public MmGuiHandler()
        {
            // Save a reference to the component as our singleton instance
            Instance = this;
        }

        public void ShowMessage(string msg)
        {
            ShowMessage(msg, MessageDisplayTime);
        }

        public void ShowMessage(string msg, float displayTime)
        {
            message = msg;

            TopRightText.text = message;
            TopRightPanel.SetActive(true);

            if (showingMessage != null)
            {
                StopCoroutine(showingMessage);
            }

            showingMessage = ShowingMessage(displayTime);
            StartCoroutine(showingMessage);
        }

        public void SetFullScreenText(string text)
        {
            fullScreenText = text;

            FullscreenText.text = fullScreenText;
        }

        public void ToggleFullScreenText()
        {
            FullscreenPanel.SetActive(!FullscreenPanel.activeSelf);
        }

        IEnumerator ShowingMessage(float displayTime)
        {
            var canvas = TopRightPanel.GetComponent<CanvasGroup>();

            msgStartingAlpha = canvas.alpha;

            yield return new WaitForSeconds(displayTime);

            while (canvas.alpha > 0)
            {
                canvas.alpha -= Time.deltaTime / MessageFadeTime;
                yield return null;
            }

            TopRightPanel.SetActive(false);

            canvas.alpha = msgStartingAlpha;

            yield return null;
        }

		public void HandleMessage(string msg)
		{
			fullScreenText = msg;

			FullscreenText.text = fullScreenText;
		}

		public void HandleSetActive(bool active)
		{
			FullscreenPanel.SetActive(active);
		}
    }
}
