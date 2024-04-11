using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MercuryMessaging;

public class T3_InputHandler : MmBaseResponder
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            GetRelayNode().MmInvoke(MmMethod.Initialize);
        }
    }

    public void ChangeColor()
    {
        GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.3f, 0.1f);
    }

    public override void MmInvoke(MmMessage message)
    {
        var type = message.MmMethod;

        switch (type)
        {
            case (MmMethod) T3_myMethods.Collision:
                ChangeColor();
                break;
            default:
                base.MmInvoke(message);
                break;

        }
    }
}
