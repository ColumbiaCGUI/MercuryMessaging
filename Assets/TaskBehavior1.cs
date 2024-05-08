using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

using Fusion;
using MercuryMessaging;

public class TaskBehavior1 : NetworkBehaviour
{
    // candle light
    public GameObject candleLight;

    public List<GameObject> candles;

    // public TextMeshProUGUI taskText;

    private string Text= "Task2\n Snap the Pot";

    // private TaskManager parent;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedNetworkUpdate()
    {
        bool allCandlesLit = true;
        foreach(GameObject candle in candles)
        {
            if(candle.activeSelf == false)
            {
                allCandlesLit = false;
                break;
            }
        }
        if(allCandlesLit)
        {
            this.transform.parent.gameObject.GetComponent<TaskManager>().TaskIncrement(Text);
        }
    }
}
