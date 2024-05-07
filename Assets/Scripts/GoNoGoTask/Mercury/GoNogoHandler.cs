// GoNogoBallHandler.cs
using UnityEngine;
using MercuryMessaging;


public enum GoNoGoMessageMethod
{
    StartGame = 101,
}

public enum GoNoGoMessageType
{
    DefaultMessage = 1101
}
        
public class GoNogoBallHandler : MmBaseResponder
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
    
    void Start()
    {
    }



    void CheckInput(bool isGreen)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // gameController.AdjustScore(isGreen);
        }
    }

    void StartGame()
    {
        
    }
    
    
}