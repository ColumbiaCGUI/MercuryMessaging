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

namespace MercuryMessaging.Support.Interpolators
{
    public abstract class Interpolator<T> : MonoBehaviour
    {
        public void Awake()
        {
            Finished = true;
        }

        public bool Finished { get; private set; }
        public bool Paused { get; set; }

        public delegate void OnFinishCallback();

        public OnFinishCallback OnFinish { get; set; }

        protected float DurationCurrent;

        public T ValueOriginal { get; set; }
        public T ValueTarget { get; set; }

        protected T valueBegin;
        protected T valueEnd;

        public T ValueCurrent { get; protected set; }

        public float Duration { get; set; }

        Coroutine coroutine;

        public virtual void Reset()
        {
            DurationCurrent = 0;
        }

        public virtual void Stop()
        {
            Finished = Paused = true;

            if (coroutine != null) StopCoroutine(coroutine);
        }

        public virtual void Begin()
        {
            Stop();

            Reset();

            Finished = Paused = false;
		
            //Debug.LogFormat("Starting - Begin:{0} End:{1}", ValueOriginal, ValueTarget);

            coroutine = StartCoroutine(Progress());
        }

        public IEnumerator Progress()
        {
            while (DurationCurrent < Duration)
            {
                if (!Paused)
                {
                    DurationCurrent += Time.deltaTime;
                    Apply();
                }
                yield return null;
            }

            Finished = true;

            if (OnFinish != null)
                OnFinish();
        }

        public virtual void Forward()
        {
            valueBegin = ValueOriginal;
            valueEnd = ValueTarget;
        }

        public virtual void Backward()
        {
            valueBegin = ValueTarget;
            valueEnd = ValueOriginal;
        }

        public virtual void Reverse(OnFinishCallback onFinish = null)
        {
            var temp = ValueOriginal;
            ValueOriginal = ValueTarget;
            ValueTarget = temp;

            DurationCurrent = (Duration - DurationCurrent);

            //Debug.LogFormat("Reversing - Begin:{0} End:{1}", ValueBegin, ValueEnd);

            OnFinish = onFinish;        
        }

        public abstract void Apply();

        public abstract void Revert();
    }
}