using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MercuryMessaging;

public class LightGUIResponder : MmBaseResponder {

	public Text UIText;

	public override void SetActive(bool active)
	{
		if(!active)
		{
			UIText.text = "Light Off";
		}
		else 
		{
			UIText.text = "Light On";
		}
	}
}
