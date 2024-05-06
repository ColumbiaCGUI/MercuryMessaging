// GoNogoBallHandler.cs
using UnityEngine;
using MercuryMessaging;

public class GoNogoBallHandler : MmBaseResponder
{
    private GoNogoGameController gameController;

    void Start()
    {
        gameController = FindObjectOfType<GoNogoGameController>(); // Find the game controller in the scene
    }

    public override void MmInvoke(MmMessage message)
    {
        if (message.MmMethod == (MmMethod)100)
        {
            GoNogoColorMessage colorMessage = message as GoNogoColorMessage;
            GetComponent<MeshRenderer>().material.color = colorMessage.value;
            CheckInput(colorMessage.value == Color.green);
        }
        else
        {
            base.MmInvoke(message);
        }
    }

    void CheckInput(bool isGreen)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gameController.AdjustScore(isGreen);
        }
    }
    
    
}