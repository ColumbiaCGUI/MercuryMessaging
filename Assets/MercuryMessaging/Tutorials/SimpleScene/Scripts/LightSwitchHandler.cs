using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitchHandler : MonoBehaviour {

	Material lightSwitchMaterial;

	void Start()
	{
		lightSwitchMaterial = GetComponent<MeshRenderer> ().material;
	}

	public void UpdateColor(bool activeState)
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
