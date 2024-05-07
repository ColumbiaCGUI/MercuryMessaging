
using MercuryMessaging;

public class GoNoGoMessage : MmMessage
{
    public bool message;
    
    public GoNoGoMessage(bool iVal,
        MmMethod mmMethod,
        MmMessageType mmMType,
        MmMetadataBlock metadataBlock)
        : base(metadataBlock, mmMType)
    {
        message = iVal;
        MmMethod = mmMethod;
        MmMessageType = mmMType;
        MetadataBlock = metadataBlock;
    }

    public override MmMessage Copy()
    {
        GoNoGoMessage newMsg = new GoNoGoMessage(message, MmMethod, MmMessageType, MetadataBlock);
        return newMsg;
    }
}