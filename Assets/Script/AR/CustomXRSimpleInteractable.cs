using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MercuryMessaging;

public class CustomXRSimpleInteractable : XRSimpleInteractable
{
    private GameManager gameManager;

    private List<string> messageInList = new List<string>();

    private List<string> messageOutList = new List<string>();

    private bool isSelected = false;

    private bool updated = false;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Update()
    {
        if(this.gameObject.GetComponent<MmRelayNode>() !=null)
        {
            // Debug.Log("CustomXRSimpleInteractable: OnSelectEntered");

            messageInList = this.gameObject.GetComponent<MmRelayNode>().messageInList;
            messageOutList = this.gameObject.GetComponent<MmRelayNode>().messageOutList;
        }

        if(isSelected && updated == false)
        {
            gameManager.ShowMessage(messageInList, true);
            gameManager.ShowMessage(messageOutList, false);
            updated = true;
        }
        else if(isSelected)
        {
            gameManager.UpdateCurrentMessage(messageInList, true);
            gameManager.UpdateCurrentMessage(messageOutList, false);
        }

    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args); // Call the base class implementation

        gameManager.MessageIn.SetActive(true);
        gameManager.MessageOut.SetActive(true);

        isSelected = true;

        // gameManager.ShowMessage(messageInList, true);
        // gameManager.ShowMessage(messageOutList, false);

    }




    // Override the OnHoverExited method
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args); // Call the base class implementation

        gameManager.MessageIn.SetActive(false);
        gameManager.MessageOut.SetActive(false);

        isSelected = false;
        updated = false;
    }
}
