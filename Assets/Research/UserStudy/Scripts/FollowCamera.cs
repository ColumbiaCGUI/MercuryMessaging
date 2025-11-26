using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    private Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // ensure that the floating speech bubble always face the camera
        camera = Camera.main;
        transform.LookAt(camera.transform);
        transform.Rotate(new Vector3(0, 180, 0)); 
    }
}
