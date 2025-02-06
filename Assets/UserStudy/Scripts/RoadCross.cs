using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadCross : MonoBehaviour
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
        EventSystem.Instance.OnTrafficLightChange += checkCrossState; 
    }

    // Update is called once per frame
    void Update()
    {
        pedestrianWalk(); 
    }

    public void checkCrossState(string direction1Color, string direction2Color) {
        // Pedestrian is currently in the crossroads 
        if (transform.position.x > crossRoadEnd && transform.position.x < crossRoadStart) {
            // if the light is turning red for the pedestrian while on the cross road
            if (direction2Color == "Red" || direction2Color == "Yellow") {
                float distance2End = transform.position.x - crossRoadEnd;
                speed = Mathf.Max(distance2End / 1.5f, 0.5f); 
                pedestrianAnimator.SetFloat("speed", speed);
            }
        }
        // Pedestrian has not crossed the road yet
        else if (transform.position.x > crossRoadStart) { 
            // if the light is turning red for the pedestrian
            if (direction2Color == "Red" || direction2Color == "Yellow") {
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
        }
    }

    void pedestrianWalk() {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        if (transform.position.x <= -61.0f) {
            EventSystem.Instance.OnTrafficLightChange -= checkCrossState;  // remove listener when pedestrian is destroyed
            Destroy(gameObject); 
        }
        if (transform.position.x <= destinationX) {
            speed = 0.0f; 
            pedestrianAnimator.SetFloat("speed", 0.0f);  // stop moving
        }
    }
}
