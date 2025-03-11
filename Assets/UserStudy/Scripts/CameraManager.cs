using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera[] cameras; 
    private Camera currentCamera;
    private int currentIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        currentCamera = cameras[currentIndex];
        currentCamera.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) {
            PreviousView();
        } else if (Input.GetKeyDown(KeyCode.E)) {
            NextView(); 
        }
    }

    void NextView() 
    {
        currentIndex = (currentIndex + 1 + cameras.Length) % cameras.Length;
        currentCamera.gameObject.SetActive(false);
        currentCamera = cameras[currentIndex];
        currentCamera.gameObject.SetActive(true);
    }

    void PreviousView() 
    {
        currentIndex = (currentIndex - 1 + cameras.Length) % cameras.Length;
        currentCamera.gameObject.SetActive(false);
        currentCamera = cameras[currentIndex];
        currentCamera.gameObject.SetActive(true);
    }
}
