// GoNogoColorMessage.cs
using UnityEngine;
using MercuryMessaging;

public class GoNogoColorMessage : MmMessage
{
    public Color value;  // The color to flash

    public GoNogoColorMessage(Color iVal, MmMethod mmMethod, MmMessageType mmMType, MmMetadataBlock metadataBlock)
        : base(metadataBlock, mmMType)
    {
        value = iVal;
        MmMethod = mmMethod;
        MmMessageType = mmMType;
        MetadataBlock = metadataBlock;
    }

    public override MmMessage Copy()
    {
        return new GoNogoColorMessage(this.value, this.MmMethod, this.MmMessageType, this.MetadataBlock);
    }
}