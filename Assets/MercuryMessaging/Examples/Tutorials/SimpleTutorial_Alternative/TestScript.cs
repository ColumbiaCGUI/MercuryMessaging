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
using UnityEngine;

namespace MercuryMessaging.Tutorials.Tutorial1
{
    public class TestScript : MmBaseResponder {

        MmRelayNode _myRelayNode;
        bool activeSet = true;

        public MmRelaySwitchNode TutorialRelaySwitch;

        // Use this for initialization
        public override void Start () {
            base.Start();

            _myRelayNode = GetComponent<MmRelayNode>();

            _myRelayNode.MmInvoke(MmMethod.Initialize, 
                new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren, 
                    MmActiveFilter.All));
        }
	
        // Update is called once per frame
        void Update () {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Space");
                activeSet = !activeSet;
                _myRelayNode.MmInvoke(MmMethod.SetActive, activeSet, 
                    new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All));
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TutorialRelaySwitch.MmInvoke(MmMethod.Switch, "Item1", 
                    new MmMetadataBlock(MmLevelFilter.Self, MmActiveFilter.All));
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TutorialRelaySwitch.MmInvoke(MmMethod.Switch, "Item2",
                    new MmMetadataBlock(MmLevelFilter.Self, MmActiveFilter.All));
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TutorialRelaySwitch.MmInvoke(MmMethod.Switch, "Item3",
                    new MmMetadataBlock(MmLevelFilter.Self, MmActiveFilter.All));
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            Debug.Log("Initialization Called");
            _myRelayNode.MmInvoke(MmMethod.SetActive, true,
                new MmMetadataBlock(MmTag.Tag0, MmLevelFilter.Child, MmActiveFilter.All));
        }
    }
}
