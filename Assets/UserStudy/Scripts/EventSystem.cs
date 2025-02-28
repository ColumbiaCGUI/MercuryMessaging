using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class EventSystem : MonoBehaviour
{
    // Singleton pattern for easy access
    public static EventSystem Instance;

    // Traffic Light Change Event (String = Light Color)
    public event Action<string, string> OnTrafficLightChange;
    public event Action<float, GameObject> onSentimentChange;
    public event Action<string, int> onStatusChange; 
    public float phase1Duration = 10.0f;
    public float phase2Duration = 3.0f; 


    private void Awake()
    {
        // Initialize the event system using singleton pattern
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    // Start the traffic light cycle
    private void Start()
    {
        StartCoroutine(TrafficLightCycle());
    }

    // Coroutine to simulate traffic light cycle
    private IEnumerator TrafficLightCycle() {
        while (true) {
            ChangeTrafficLight("Red", "Green");
            yield return new WaitForSeconds(phase1Duration);

            ChangeTrafficLight("Red", "Yellow");
            yield return new WaitForSeconds(phase2Duration);

            ChangeTrafficLight("Green", "Red");
            yield return new WaitForSeconds(phase1Duration);

            ChangeTrafficLight("Yellow", "Red");
            yield return new WaitForSeconds(phase2Duration);
        }
    }

    // Method to trigger the event
    public void ChangeTrafficLight(string direction1Color, string direction2Color)
    {
        OnTrafficLightChange?.Invoke(direction1Color, direction2Color);
    }

    public void InduceFear(float amount, GameObject pedestrian) {
        onSentimentChange?.Invoke(amount, pedestrian);
    }

    public void dispellFear(float amount = 0, GameObject pedestrian = null) {
        onSentimentChange?.Invoke(amount, pedestrian); 
    }

    public void updateStatus(string type, int value) {
        onStatusChange?.Invoke(type, value); 
    }

}
