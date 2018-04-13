using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MercuryMessaging;

public class HandController : MmBaseResponder {

	private bool activeState = false;

	void OnTriggerEnter(Collider col)
	{
		activeState = !activeState;

		GetRelayNode().MmInvoke (MmMethod.SetActive, activeState, 
			new MmMetadataBlock (MmLevelFilter.Child, MmActiveFilter.All));
	}
}
