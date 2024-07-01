using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MercuryMessaging;

public class T4_CubeResponder : MonoBehaviour
{
    // // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }

    public void ChangeColor(Color col)
    {
        GetComponent<MeshRenderer>().material.color = col;
    }

    
}
