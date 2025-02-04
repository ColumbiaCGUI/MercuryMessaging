using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadCross : MonoBehaviour
{
    public float speed = 1.0f; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
