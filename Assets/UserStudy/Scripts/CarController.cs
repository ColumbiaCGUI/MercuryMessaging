using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class CarController : MonoBehaviour
{
    public float destination; 
    public float crossRoadStart;
    public float crossRoadEnd;
    public float speed = 2.0f;  // Car's speed
    public float recklessness = 0.0f; // determine if a car will rush a yellow light or not
    public bool direction1 = true; 
    private float despawnPoint; 
    private bool inTriggerZone = false; 

    // Start is called before the first frame update
    void Start()
    {
        TrafficEventManager.Instance.onVehicleCross += CrossRoad; 

        if (direction1) {
            despawnPoint = StreetInfo.Instance.carDespawnLocationZ; 
        } else {
            despawnPoint = StreetInfo.Instance.carDespawnLocationX; 
        }
        destination = despawnPoint;
    }

    // Update is called once per frame
    void Update()
    {
        carDrive(); 
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

    public void checkCrossState(string color, float position) {
        // Car has not crossed the road yet
        if (position >= crossRoadStart + 2.7f) { 
            if (color == "Green") {
                destination = crossRoadEnd - 100;  // move to the end of the road
            } else {
                destination = crossRoadStart + 4.5f; 
            }
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
        if (intersection == 1) {
            if (direction1) {
                if (transform.position.z <= StreetInfo.Instance.direction1Intersection1End) {
                    return false; 
                }
            } else {
                if (transform.position.x <= StreetInfo.Instance.direction2Intersection1End) {
                    return false; 
                }
            }
        } else {
            if (direction1) {
                if (transform.position.z > StreetInfo.Instance.direction1Intersection1End) {
                    return false; 
                }
            } else {
                if (transform.position.x > StreetInfo.Instance.direction2Intersection1End) {
                    return false; 
                }
            }
        }
        return true;  
    }

    public void carDrive() {
        transform.Translate(Vector3.forward * speed * Time.deltaTime); 
        float position; 
        if (direction1) {
            position = transform.position.z; 
        } else {
            position = transform.position.x;
        }

        if (position <= despawnPoint) {
            Destroy(gameObject); 
            TrafficEventManager.Instance.onVehicleCross -= CrossRoad;
        }
        
        if (inTriggerZone) {
            speed = 0.0f; 
        } else {
            if (position <= destination) {
                speed = 0; 
            } else {
                speed = 2.0f; 
            }
        }
    } 

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Car")) {
            CarController carController = other.gameObject.GetComponent<CarController>(); 
            if (carController.direction1 == direction1) {
                if (direction1) {
                    if (other.gameObject.transform.position.z < transform.position.z) {
                        inTriggerZone = true; 
                    }
                } else {
                    if (other.gameObject.transform.position.x < transform.position.x) {
                        inTriggerZone = true; 
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Car")) {
            CarController carController = other.gameObject.GetComponent<CarController>(); 
            if (carController.direction1 == direction1) {
                if (direction1) {
                    if (other.gameObject.transform.position.z < transform.position.z) {
                        inTriggerZone = false; 
                    }
                } else {
                    if (other.gameObject.transform.position.x < transform.position.x) {
                        inTriggerZone = false; 
                    }
                }
            }
        }
    }
}
