using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MercuryMessaging;

public enum T3_myMethods
{
    Collision = 100
}

public class T3_SphereResponder : MmBaseResponder
{

    public override void Initialize()
    {
        GetComponent<Rigidbody>().useGravity = true;
    }

    public void OnCollisionEnter()
    {
        GetRelayNode().MmInvoke((MmMethod)T3_myMethods.Collision, new MmMetadataBlock(MmLevelFilter.Parent));
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
