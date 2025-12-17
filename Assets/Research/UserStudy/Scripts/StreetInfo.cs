using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetInfo : MonoBehaviour
{
    public static StreetInfo Instance; 

    public float humanDespawnLocationX = -98.0f; 
    public float humanSpawnLocationX = -40.0f; 
    public float humanDespawnLocationZ = 47.0f; 
    public float humanSpawnLocationZ = 89.0f; 
    public float carDespawnLocationZ;
    public float carSpawnLocationZ; 
    public float carDespawnLocationX; 
    public float carSpawnLocationX; 
    public float direction1Intersection1Start; 
    public float direction1Intersection1End; 
    public float direction1Intersection2Start;
    public float direction1Intersection2End; 
    public float direction2Intersection1Start = -51.0f;
    public float direction2Intersection1End = -57.0f;
    public float direction2Intersection2Start = -86.0f;
    public float direction2Intersection2End = -94.0f;

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
}
