using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drawing;

public class ArrowDrawer : MonoBehaviour
{
    public Transform start;

    public Transform end;

    public Color arrowColor = Color.red;
    public float arrowHeadLength = 0.25f;
    public float arrowHeadAngle = 20.0f;
    public float lineWidth = 0.02f;
    public float duration = 2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    

    void Update()
    {
        // var draw = Draw.ingame;
        // Vector3 distance = end.position - start.position;

        // int numArrows = (int)(distance.magnitude / 0.35f);

        // for (int i = 1; i < numArrows-1; i++)
        // {
        //     Vector3 pointA = Vector3.Lerp(start.position, end.position, i / (float)numArrows);
            
        //     draw.Arrowhead(pointA, distance.normalized, 0.1f, Color.red);
        // }
        
        // draw.ArrowheadArc(start.position, end.position, 0.05f, 30f, Color.red );
        
    }
}
