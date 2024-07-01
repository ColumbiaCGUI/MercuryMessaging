
using MercuryMessaging;

public class GoNoGoMessage : MmMessage
{
    public GoNogoTrialData trialData;
    
    public GoNoGoMessage(GoNogoTrialData iVal,
        MmMethod mmMethod,
        MmMessageType mmMType,
        MmMetadataBlock metadataBlock)
        : base(metadataBlock, mmMType)
    {
        trialData = iVal;
        MmMethod = mmMethod;
        MmMessageType = mmMType;
        MetadataBlock = metadataBlock;
    }

    public override MmMessage Copy()
    {
        GoNoGoMessage newMsg = new GoNoGoMessage(trialData, MmMethod, MmMessageType, MetadataBlock);
        return newMsg;
    }
}