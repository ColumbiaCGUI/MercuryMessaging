using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLabelLine : MonoBehaviour

{
    // Apply these values in the editor
    private LineRenderer Line;
    public GameObject linePrefab;
    public Vector3 startPos;
    public Vector3 endPos;
    public float startWidth;
    public float endWidth;
    public string parentName;
   
    void Start()
    {
        // set the color of the line
        GameObject lineObj = Instantiate(linePrefab, transform.position, Quaternion.Euler(0, 0, 0));

        Line = lineObj.GetComponent<LineRenderer>();

        Line.startColor = Color.black;
        Line.endColor = Color.black;

        Line.startWidth = startWidth;
        Line.endWidth = endWidth;
        Line.positionCount = 2;

        Line.SetPosition(0, startPos);          
        Line.SetPosition(1, endPos);

        lineObj.transform.parent = GameObject.Find(parentName).transform;
    }
}