// BallFlash.cs
using UnityEngine;
using MercuryMessaging;

public class BallFlash : MmBaseResponder
{
    public Material greenMaterial;
    public Material redMaterial;
    public Material defaultMaterial;
    public string currentColor;

    public override void MmInvoke(MmMessage message)
    {
        Color color = ((GoNogoColorMessage) message).value;
        GetComponent<Renderer>().material.color = color;

        // if (color == "Green")
        // {
        //     GetComponent<Renderer>().material = greenMaterial;
        //     currentColor = "Green";
        // }
        // else if (color == "Red")
        // {
        //     GetComponent<Renderer>().material = redMaterial;
        //     currentColor = "Red";
        // }
        Invoke("ResetMaterial", 0.5f); // flash duration
    }

    private void ResetMaterial()
    {
        GetComponent<Renderer>().material = defaultMaterial;
        currentColor = null;
    }
}