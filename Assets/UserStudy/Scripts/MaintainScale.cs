using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaintainScale : MonoBehaviour
{
    public float originalDistance = 8.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float distance = (transform.position - Camera.main.transform.position).magnitude;
        transform.localScale = (distance / originalDistance) * Vector3.one; 
        transform.localPosition = new Vector3(transform.localPosition.x, 2 + 1 * distance / originalDistance, transform.localPosition.z);
    }

}
