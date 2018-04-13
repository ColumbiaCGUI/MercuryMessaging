using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingScriptB : MonoBehaviour {

	public InvocationComparison EventController;

	// Use this for initialization
	void Start () {
		//EventController.OnEventTest += MyEventHandler;
	}

	public void MyEventHandler()
	{
		EventController.stopWatch.Stop ();
		EventController.simpleLock = false;
	}
}
