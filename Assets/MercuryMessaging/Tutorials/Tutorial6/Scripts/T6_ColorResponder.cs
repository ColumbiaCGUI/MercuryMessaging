using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MercuryMessaging;

public class T6_ColorResponder : MmSwitchResponder
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

    public override void MmInvoke(MmMessage message)
    {
        var type = message.MmMethod;

        switch (type)
        {
            case ((MmMethod) T6_myMethods.UpdateColor):

                Color col = ((T6_ColorMessage) message).value; 
                ChangeColor(col);

                break;
            default:
            
                base.MmInvoke(message);
                break;

        }
    }
}
