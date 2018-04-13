using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightGUIHandler : MonoBehaviour {

	public Text UIText;

	public void UpdateGUI(bool activeState)
	{
		if(!activeState)
		{
			UIText.text = "Light Off";
		}
		else 
		{
			UIText.text = "Light On";
		}
	}
}
