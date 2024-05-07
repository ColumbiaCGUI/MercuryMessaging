using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

using Fusion;
using MercuryMessaging;

public class TaskBehavior1 : MonoBehaviour
{
    // candle light
    public GameObject candleLight;

    public List<GameObject> candles;

    public TextMeshProUGUI taskText;

    private string Text= "Task1\n Turn on the Candle";
    // Start is called before the first frame update
    void Start()
    {
        taskText.text = Text;
    }

    // Update is called once per frame
    void Update()
    {
        taskText.text = Text;

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
            this.transform.parent.gameObject.GetComponent<TaskManager>().TaskNumber++;
        }
    }
}
