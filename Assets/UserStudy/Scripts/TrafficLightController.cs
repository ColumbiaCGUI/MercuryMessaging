using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    private Renderer lightRenderer; 
    public bool direction1 = true; 
    private Vector2 redGreen = new Vector2(0, 0); 
    private Vector2 redYellow = new Vector2(0.0625f, 0); 
    private Vector2 greenRed = new Vector2(0.125f, 0); 
    private Vector2 yellowRed = new Vector2(0.1875f, 0);


    // Start is called before the first frame update
    void Start()
    {
        lightRenderer = GetComponent<Renderer>();
        EventSystem.Instance.OnTrafficLightChange += TrafficLightControl; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable() {
        EventSystem.Instance.OnTrafficLightChange -= TrafficLightControl; 
    }

    void TrafficLightControl(string direction1Color, string direction2Color) {
        if (direction1) {
            if (direction1Color == "Red" && direction2Color == "Green") {
                lightRenderer.materials[1].SetTextureOffset("_MainTex", redGreen);
            } else if (direction1Color == "Yellow" && direction2Color == "Red") {
                lightRenderer.materials[1].SetTextureOffset("_MainTex", yellowRed);
            } else if (direction1Color == "Red" && direction2Color == "Yellow") {
                lightRenderer.materials[1].SetTextureOffset("_MainTex", redYellow);
            } else {
                lightRenderer.materials[1].SetTextureOffset("_MainTex", greenRed);
            }
        } else {
            if (direction1Color == "Red" && direction2Color == "Green") {
                lightRenderer.materials[1].SetTextureOffset("_MainTex", greenRed);
            } else if (direction1Color == "Yellow" && direction2Color == "Red") {
                lightRenderer.materials[1].SetTextureOffset("_MainTex", redYellow);
            } else if (direction1Color == "Red" && direction2Color == "Yellow") {
                lightRenderer.materials[1].SetTextureOffset("_MainTex", yellowRed);
            } else {
                lightRenderer.materials[1].SetTextureOffset("_MainTex", redGreen);
            }
        }
        EventSystem.Instance.ControlPedestrian(direction2Color); 
        EventSystem.Instance.ControlVehicle(direction1Color); 

    }
}
