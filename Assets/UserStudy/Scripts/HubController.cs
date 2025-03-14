using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HubController : MonoBehaviour
{
    public TMP_Text fearText; 
    public TMP_Text averageText;
    public int totalHuman = 0; 
    public int totalFear = 0; 

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
        fearText.text = "Total Rushness: " + totalFear.ToString();

        float averageFear = totalFear / totalHuman;
        averageText.text = "Average Rushness: " + averageFear.ToString();
    }

    void UpdatePopulation(int value) {
        // Update total human population
        totalHuman += value; 
        
        float averageFear = totalFear / totalHuman;
        averageText.text = "Average Rushness: " + averageFear.ToString(); 
    }
}
