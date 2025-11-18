using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class SentimentController : MonoBehaviour
{
    public TMP_Text sentiment; 
    private int currentFear; 

    // Start is called before the first frame update
    void Start()
    {
        if (TrafficEventManager.Instance != null)
        {
            TrafficEventManager.Instance.onSentimentChange += DisplayFearFactor;
        }
        else
        {
            Debug.LogError("TrafficEventManager.Instance is null in SentimentController.Start()");
        }

        if (sentiment != null)
        {
            sentiment.text = "Pace " + 0 + "\n";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        // Unsubscribe when this GameObject is destroyed
        if (TrafficEventManager.Instance != null)
        {
            TrafficEventManager.Instance.onSentimentChange -= DisplayFearFactor;
        }
    }

    void DisplayFearFactor(float amount, GameObject pedestrian) {
        if (pedestrian == gameObject) {
            int change = (int)amount - currentFear; 
            currentFear = (int)amount;
            sentiment.text = "Pace " + currentFear + "\n";
            TrafficEventManager.Instance.updateStatus("Fear", (int)change);
        }
    }
}
