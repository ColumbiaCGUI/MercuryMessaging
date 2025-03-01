using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class Pedestrian : MonoBehaviour
{
    public float speed = 0.5f; 
    private float crossRoadStart = -51.0f; 
    private float crossRoadEnd = -57.0f; 
    private Animator pedestrianAnimator; 
    private float destinationX = -100.0f; 

    // Start is called before the first frame update
    void Start()
    {
        pedestrianAnimator = GetComponent<Animator>();
        EventSystem.Instance.onPedestrianCross += checkCrossState; 
    }

    // Update is called once per frame
    void Update()
    {
        pedestrianWalk(); 
    }

    public void checkCrossState(string color) {
        // Pedestrian is currently in the crossroads 
        if (transform.position.x > crossRoadEnd && transform.position.x < crossRoadStart) {
            // if the light is turning red for the pedestrian while on the cross road
            if (color == "Red" || color == "Yellow") {
                float distance2End = transform.position.x - crossRoadEnd;
                speed = Mathf.Max(distance2End / 1.5f, 0.5f); 
                pedestrianAnimator.SetFloat("speed", speed);

                // induce fear based on the distance to the other side
                float fear = distance2End * 0.5f; 
                EventSystem.Instance.InduceFear(fear, gameObject);
            }
        }
        // Pedestrian has not crossed the road yet
        else if (transform.position.x > crossRoadStart) { 
            // if the light is turning red for the pedestrian
            if (color == "Red" || color == "Yellow") {
                destinationX = -50.0f; 
            } else {
                destinationX = -100.0f;  // move to the end of the road
                speed = 0.5f; 
                pedestrianAnimator.SetFloat("speed", speed);  // reset to normal speed
            }
        }
        // Pedestrian has crossed the road 
        else {
            speed = 0.5f; 
            pedestrianAnimator.SetFloat("speed", speed);
            EventSystem.Instance.InduceFear(0, gameObject); 
        }
    }

    void pedestrianWalk() {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        if (transform.position.x <= -61.0f) {
            EventSystem.Instance.onPedestrianCross -= checkCrossState;  // remove listener when pedestrian is destroyed
            EventSystem.Instance.updateStatus("Population", -1); 
            Destroy(gameObject); 
        }
        if (transform.position.x <= destinationX) {
            speed = 0.0f; 
            pedestrianAnimator.SetFloat("speed", 0.0f);  // stop moving
        }
    }
}
