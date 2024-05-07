using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TaskBehavior2 : MonoBehaviour
{
    public GameObject pot;

    public GameObject potShadow;
    public TextMeshProUGUI taskText;

    private string Text= "Task2\n Snap the pot";

    private bool rotationCheck = false;

    private bool positionCheck = false;
    // Start is called before the first frame update
    void Start()
    {
        taskText.text = Text;
    }

    // Update is called once per frame
    void Update()
    {
        taskText.text = Text;
        
        rotationCheck =false;
        positionCheck = false;
        Vector3 potPos = pot.transform.position;
        Vector3 potShadowPos = potShadow.transform.position;
        
        Quaternion potRot = pot.transform.rotation;
        Quaternion potShadowRot = potShadow.transform.rotation;
    }
}
