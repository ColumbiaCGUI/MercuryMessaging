using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HubController : MonoBehaviour
{
    public TMP_Text fearText; 
    public TMP_Text averageText;
    public float totalHuman = 0.0f; 
    public float totalFear = 0.0f; 

    // Start is called before the first frame update
    void Start()
    {
        EventSystem.Instance.onStatusChange += UpdateStatus; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateStatus(string type, int value) {
        if (type == "Fear") {
            UpdateFear(value); 
        } else if (type == "Population") {
            UpdatePopulation(value); 
        }
    }

    void UpdateFear(int value) {
        // Update fear value in the game UI hub
        totalFear += value;         
        fearText.text = "Total Fear: " + totalFear.ToString();

        float averageFear = totalFear / totalHuman;
        averageText.text = "Average Fear: " + averageFear.ToString("0.00");
    }

    void UpdatePopulation(int value) {
        // Update total human population
        totalHuman += value; 
        
        float averageFear = totalFear / totalHuman;
        averageText.text = "Average Fear: " + averageFear.ToString("0.00"); 
    }
}
