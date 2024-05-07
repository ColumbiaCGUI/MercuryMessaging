using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetOutline : MonoBehaviour
{
    public GameObject head;
    public GameObject leftHand;
    public GameObject rightHand;
    public void SetOutlineColor(Color color)
    {
        head.GetComponent<Outline>().OutlineColor = color;
    }

    public void SetCullingMusk()
    {
        SetGameLayerRecursive(head, 0);
        SetGameLayerRecursive(leftHand, 0);
        SetGameLayerRecursive(rightHand, 0);

    }

    private void SetGameLayerRecursive(GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach (Transform child in gameObject.transform)
        {
            SetGameLayerRecursive(child.gameObject, layer);
        }
    }

}
