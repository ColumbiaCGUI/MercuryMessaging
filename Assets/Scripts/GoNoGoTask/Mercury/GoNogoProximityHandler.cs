using UnityEngine;
using UnityEngine.UI;
using MercuryMessaging;


public class GoNogoProximityHandler : MmBaseResponder
{
    public override void MmInvoke(MmMessage message)
    {

        var type = message.MmMethod;

        switch (type)
        {
            case ((MmMethod)GoNoGoMessageMethod.StartGame):
                break;
            default:
                base.MmInvoke(message);
                break;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Make sure the player GameObject has the tag "Player"
        {
            GetRelayNode().MmInvoke(
                new GoNoGoMessage(true,
                    (MmMethod)GoNoGoMessageMethod.StartGame,
                    (MmMessageType)GoNoGoMessageType.DefaultMessage,
                    new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // instructionUI.SetActive(false);  // Hide instructions when player moves away
        }
    }

}