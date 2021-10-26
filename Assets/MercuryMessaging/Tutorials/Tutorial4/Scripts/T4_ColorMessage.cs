using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MercuryMessaging;

public class T4_ColorMessage : MmMessage
{
    public Color value;
    // Start is called before the first frame update

    public T4_ColorMessage(Color iVal,
        MmMethod mmMethod,
        MmMessageType mmMType,
        MmMetadataBlock metadataBlock)
        : base(metadataBlock, mmMType)
    {
        value = iVal;
        MmMethod = mmMethod;
        MmMessageType = mmMType;
        MetadataBlock = metadataBlock;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
