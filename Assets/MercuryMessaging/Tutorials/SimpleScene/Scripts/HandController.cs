﻿// Copyright (c) 2017-2019, Columbia University
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
using System.Collections.Generic;
using UnityEngine;
using MercuryMessaging;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class HandController : MmBaseResponder {

	// public GameObject lightMode;
	private InputActionAsset playerInput;

	private InputAction signalAction;

	private bool activeState = false;

	private float timePast;

	public float messagePeriod;

	public bool boxTriggered = true;

	void Start()
	{
		playerInput = GameObject.Find("GameManager").GetComponent<GameManager>().playerInput;
		signalAction = playerInput.FindActionMap("XRI RightHand Interaction").FindAction("Signal");
		signalAction.Enable();
	}
	public override void Update()
	{
		// set the automatic signal passing
		// if (OVRInput.GetDown(OVRInput.RawButton.A) || signalAction.triggered) {

		if(signalAction.triggered || timePast >= messagePeriod)
		{
			timePast =0;
			if(activeState)
			{
				activeState = false;
				Debug.Log("HandController: Deactivating");
			}
			else
			{
				activeState = true;
				Debug.Log("HandController: Activating");
			}

			if (signalAction.triggered) {
				boxTriggered = false;
			}
			else {
				boxTriggered = true;
			}

			GetRelayNode().MmInvoke (MmMethod.SetActive, activeState, 
				new MmMetadataBlock (MmLevelFilter.Child, MmActiveFilter.All));
		}
		timePast += Time.deltaTime;
	}
}
