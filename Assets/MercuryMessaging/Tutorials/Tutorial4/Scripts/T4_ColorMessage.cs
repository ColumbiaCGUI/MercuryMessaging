using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MercuryMessaging;

public class T4_ColorMessage : MmMessage
{
    public Color value;
    public T4_ColorMessage(Color iVal,
        MmMessageType mmMessageType = default(MmMessageType),
                MmMetadataBlock metadataBlock = null)
            : base(metadataBlock, mmMessageType)
    {
        value = iVal;
    }

    public override MmMessage Copy()
    {
        T4_ColorMessage newMessage = new T4_ColorMessage(this.value);
        newMessage.value = value;

        return newMessage;
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
