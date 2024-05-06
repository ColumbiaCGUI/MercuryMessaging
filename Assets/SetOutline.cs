using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetOutline : MonoBehaviour
{
    public GameObject head;
    public void SetOutlineColor(Color color)
    {
        head.GetComponent<Outline>().OutlineColor = color;
    }

}
