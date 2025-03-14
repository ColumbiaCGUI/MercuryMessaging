using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class Pedestrian : MonoBehaviour
{
    public float speed = 0.5f; 
    private float crossRoadStart; 
    private float crossRoadEnd; 
    private Animator pedestrianAnimator; 
    private float destination; 
    private float despawnPoint; 
    public bool direction1 = false; 

    void Start()
    {
        pedestrianAnimator = GetComponent<Animator>();
        EventSystem.Instance.onPedestrianCross += CrossRoad; 

        if (direction1) {
            despawnPoint = StreetInfo.Instance.humanDespawnLocationZ; 
        } else {
            despawnPoint = StreetInfo.Instance.humanDespawnLocationX; 
        }
        destination = despawnPoint;
    }

    // Update is called once per frame
    void Update()
    {
        pedestrianWalk(); 
    }

    public void CrossRoad(string direction1Color, string direction2Color, int intersection) {
        // check if pedestrian is in current intersection 
        if (!IsAtCurrentIntersection(intersection)) {
            return; 
        }

        SetStreetInfo(intersection);         
        if (direction1) {
            checkCrossState(direction1Color, transform.position.z); 
        } else {
            checkCrossState(direction2Color, transform.position.x); 
        }   
    }

    void checkCrossState(string color, float position) {
        // Pedestrian is currently in the crossroads 
            if (position > crossRoadEnd && position < crossRoadStart) {
                // if the light is turning red for the pedestrian while on the cross road
                if (color == "Red" || color == "Yellow") {
                    float distance2End = position - crossRoadEnd;
                    speed = Mathf.Max(distance2End / 1.5f, 0.5f); 
                    pedestrianAnimator.SetFloat("speed", speed);

                    // induce fear based on the distance to the other side
                    float fear = distance2End * 0.5f; 
                    EventSystem.Instance.InduceFear(fear, gameObject);
                }
            }
            // Pedestrian has not crossed the road yet
            else if (position > crossRoadStart) { 
                // if the light is turning red for the pedestrian
                if (color == "Red" || color == "Yellow") {
                    destination = crossRoadStart + 1; 
                } else {
                    destination = crossRoadEnd - 50;  // move to the end of the road
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

    void SetStreetInfo(int intersection) {
        if (direction1) {
            if (intersection == 1) {
                crossRoadStart = StreetInfo.Instance.direction1Intersection1Start; 
                crossRoadEnd = StreetInfo.Instance.direction1Intersection1End; 
            } else { 
                crossRoadStart = StreetInfo.Instance.direction1Intersection2Start; 
                crossRoadEnd = StreetInfo.Instance.direction1Intersection2End; 
            }
        } else {
            if (intersection == 1) {
                crossRoadStart = StreetInfo.Instance.direction2Intersection1Start; 
                crossRoadEnd = StreetInfo.Instance.direction2Intersection1End;
            } else {
                crossRoadStart = StreetInfo.Instance.direction2Intersection2Start; 
                crossRoadEnd = StreetInfo.Instance.direction2Intersection2End; 
            }
        }
    }

    bool IsAtCurrentIntersection(int intersection) {
        float bound2 = (StreetInfo.Instance.direction2Intersection1End + StreetInfo.Instance.direction2Intersection2Start) / 2; 
        float bound1 = (StreetInfo.Instance.direction1Intersection1End + StreetInfo.Instance.direction1Intersection2Start) / 2; 

        if (intersection == 1) {
            if (direction1) {
                if (transform.position.z <= bound1) {
                    return false; 
                }
            } else {
                if (transform.position.x <= bound2) {
                    return false; 
                }
            }
        } else {
            if (direction1) {
                if (transform.position.z > bound1) {
                    return false; 
                }
            } else {
                if (transform.position.x > bound2) {
                    return false; 
                }
            }
        }
        return true;  // pedestrian is in the current intersection
    }

    void pedestrianWalk() {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        float position; 
        if (direction1) {
            position = transform.position.z; 
        } else {
            position = transform.position.x;
        }

        if (position <= despawnPoint) {
            // remove listener when pedestrian is destroyed
            EventSystem.Instance.onPedestrianCross -= CrossRoad; 

            EventSystem.Instance.updateStatus("Population", -1); 
            Destroy(gameObject); 
        }

        if (position <= destination) {
            speed = 0.0f; 
            pedestrianAnimator.SetFloat("speed", 0.0f);  // stop moving
        }
    }
}
