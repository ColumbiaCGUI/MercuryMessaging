using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class CarController : MonoBehaviour
{
    public float destinationZ = 50; 
    public float crossRoadStartZ = 81.0f;
    public float crossRoadEndZ = 72.0f;
    public float speed = 2.0f;  // Car's speed
    public float recklessness = 0.0f; // determine if a car will rush a yellow light or not

    // Start is called before the first frame update
    void Start()
    {
        EventSystem.Instance.OnTrafficLightChange += checkCrossState; 
    }

    // Update is called once per frame
    void Update()
    {
        carDrive(); 
    }

    public void checkCrossState(string direction1Color, string direction2Color) {
        // Car has not crossed the road yet
        if (transform.position.z > crossRoadStartZ) { 
            // if the light is turning red or yellow
            if (direction1Color == "Red") {
                destinationZ = 83; 
            } else if (direction1Color == "Green") {
                destinationZ = 50.0f;  // move to the end of the road
            } else {
                float level = Random.Range(0.0f, 1.0f); 
                if (level <= recklessness) {
                    destinationZ = 50.0f;  // rush to the end of the road
                } else {
                    destinationZ = 83.0f;  // move to the start of the road
                }
            }
        }
    }

    public void carDrive() {
        transform.Translate(Vector3.forward * speed * Time.deltaTime); 
        if (transform.position.z <= 60) {
            Destroy(gameObject); 
            EventSystem.Instance.OnTrafficLightChange -= checkCrossState;
        }
        if (transform.position.z <= destinationZ) {
            speed = 0; 
        } else {
            speed = 2; 
        }

    }
}
