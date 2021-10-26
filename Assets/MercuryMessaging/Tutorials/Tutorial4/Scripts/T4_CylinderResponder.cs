using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MercuryMessaging;

public class T4_CylinderResponder : MmBaseResponder
{
    // Start is called before the first frame update
    public void ChangeColor(Color col)
    {
        GetComponent<MeshRenderer>().material.color = col;
    }

    public override void MmInvoke(MmMessage message)
    {

        var type = message.MmMethod;

        switch (type)
        {
            case ((MmMethod)T4_myMethods.UpdateColor):
                Debug.Log("CASTING MESSAGE");
                Color col = ((T4_ColorMessage) message).value;
                ChangeColor(col);
                break;
            default:
                base.MmInvoke(message);
                break;
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}