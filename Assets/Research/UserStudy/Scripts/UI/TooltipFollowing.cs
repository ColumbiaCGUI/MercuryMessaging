// Suppress MM008: This class manages UI line rendering, not MercuryMessaging responders
// SetParent is used for Unity hierarchy organization, not MM routing
#pragma warning disable MM008

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

// From: https://github.com/Firnox/Billboarding/blob/main/Billboard.cs

public class TooltipFollowing : MonoBehaviour
{
    public Transform Button;
    // public Vector3 ButtonPos;
    public GameObject linePrefab;
    private LineRenderer lineRenderer;
    public Material lineMaterial;
    public Color lineColor;

    void Start()
    {
        GameObject line = Instantiate(linePrefab);
        
        // line.transform.SetParent(GameObject.Find("Map/Map").transform);
        line.transform.SetParent(this.transform.parent);

        line.transform.localPosition = new Vector3(0, 0, 0);
        line.transform.rotation = new Quaternion(0, 0, 0, 0);
        line.transform.localScale = new Vector3(1, 1, 1);

        lineRenderer = line.GetComponent<LineRenderer>();
        

        // lineRenderer = linePrefab.GetComponent<LineRenderer>(); 
        lineRenderer.positionCount = 2;
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = 0.0015f;
        lineRenderer.endWidth = 0.0015f;
        lineRenderer.material.color =lineColor;
    }

    void LateUpdate()
    {
        //  Snap to map
        // RaycastHit hit;
        // Physics.Raycast(transform.position + Vector3.up * 30f, Vector3.down, out hit, Mathf.Infinity, mapLayer);
        // transform.position = hit.point;
        // if(PrevLine !=null )
        // {
        //     Destroy(PrevLine);
        // }

        transform.LookAt(Camera.main.transform.position, Vector3.up);
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

        lineRenderer.SetPosition(0, transform.localPosition);
        // lineRenderer.SetPosition(1, ButtonPos);
        lineRenderer.SetPosition(1, Button.localPosition);

    }
}
