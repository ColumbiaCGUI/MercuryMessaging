using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MercuryMessaging;

public class LightSwitchResponder : MmBaseResponder {

	Material lightSwitchMaterial;

	public override void Start()
	{
		lightSwitchMaterial = GetComponent<MeshRenderer> ().material;
	}

	public override void SetActive(bool activeState)
	{
		if(activeState)
		{
			lightSwitchMaterial.color = Color.red;
		}
		else
		{
			lightSwitchMaterial.color = Color.white;
		}
	}
}
