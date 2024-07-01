
using MercuryMessaging;

public class CandleMessage : MmMessage
{
    public bool interacted;
    
    public CandleMessage(bool iVal,
        MmMethod mmMethod,
        MmMessageType mmMType,
        MmMetadataBlock metadataBlock)
        : base(metadataBlock, mmMType)
    {
        interacted = iVal;
        MmMethod = mmMethod;
        MmMessageType = mmMType;
        MetadataBlock = metadataBlock;
    }

    public override MmMessage Copy()
    {
        CandleMessage newMsg = new CandleMessage(interacted, MmMethod, MmMessageType, MetadataBlock);
        return newMsg;
    }
}