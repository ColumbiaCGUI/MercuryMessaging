using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandController_NonMercury : MonoBehaviour {

	GameObject LightSwitch;
	GameObject LightBulb;

	LightGUIHandler GUIHandler;
	LightSwitchHandler SwitchHandler;

	private bool activeState = false;

	void OnTriggerEnter(Collider col)
	{
		activeState = !activeState;

		LightSwitch.SetActive (activeState);

		LightBulb.SetActive (activeState);

		GUIHandler.UpdateGUI (activeState);

		SwitchHandler.UpdateColor (activeState);
	}
}
