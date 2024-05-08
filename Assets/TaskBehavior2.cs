using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

using Fusion;
public class TaskBehavior2 : NetworkBehaviour
{
    public GameObject pot;

    public GameObject potShadow;

    private string Text= "Task3\n Go-no-go task";

    private bool rotationCheck = false;

    private bool positionCheck = false;

    public XRBaseInteractor leftRay;
    public XRBaseInteractor rightRay;

    public XRBaseInteractor leftHand;

    public XRBaseInteractor rightHand;

    public AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedNetworkUpdate()
    {
        // var leftRayTarget = leftRay.selectTarget;
        // var rightRayTarget = rightRay.selectTarget;
        // var leftHandTarget = leftHand.selectTarget;
        // var rightHandTarget = rightHand.selectTarget;

        // bool leftRaySelect = false;
        // bool rightRaySelect = false;
        // bool leftHandSelect = false;
        // bool rightHandSelect = false;

        // if(leftRayTarget != null)
        // {
        //     GameObject target = leftRayTarget.gameObject;
        //     if(target == potShadow)
        //     {
        //         leftRaySelect = true;
        //     }
        // }
        // if(rightRayTarget != null)
        // {
        //     GameObject target = rightRayTarget.gameObject;
        //     if(target == potShadow)
        //     {
        //         rightRaySelect = true;
        //     }
        // }
        // if(leftHandTarget != null)
        // {
        //     GameObject target = leftHandTarget.gameObject;
        //     if(target == potShadow)
        //     {
        //         leftHandSelect = true;
        //     }
        // }
        // if(rightHandTarget != null)
        // {
        //     GameObject target = rightHandTarget.gameObject;
        //     if(target == potShadow)
        //     {
        //         rightHandSelect = true;
        //     }
        // }
        potShadow.SetActive(true);

        rotationCheck =false;
        positionCheck = false;
        Vector3 potPos = pot.transform.position;
        Vector3 potShadowPos = potShadow.transform.position;
        
        Quaternion potRot = pot.transform.rotation;
        Quaternion potShadowRot = potShadow.transform.rotation;

        if(Quaternion.Angle(potRot, potShadowRot) < 15f)
        {
            rotationCheck = true;
        }
        if(Vector3.Distance(potPos, potShadowPos) < 0.2f)
        {
            positionCheck = true;
        }

        if(rotationCheck && positionCheck)
        {   
            // if(leftRaySelect)
            // {
            //     leftRay.EndManualInteraction();
            // }
            // if(rightRaySelect)
            // {
            //     rightRay.EndManualInteraction();
            // }
            // if(leftHandSelect)
            // {
            //     leftHand.EndManualInteraction();
            // }
            // if(rightHandSelect)
            // {
            //     rightHand.EndManualInteraction();
            // }
            pot.GetComponent<XRGrabInteractable>().enabled = false;
            potShadow.SetActive(false);

            pot.transform.position = potShadow.transform.position;
            pot.transform.rotation = potShadow.transform.rotation;
            audioSource.Play(0);
            // if(this.transform.parent.gameObject.GetComponent<TaskManager>().wristMenu.activeSelf == false)
            // {
            //     // Debug.Log("Task1 completed"
            //     this.transform.parent.gameObject.GetComponent<TaskManager>().wristMenu.SetActive(true);
            // }
            // else
            // {
            //     this.transform.parent.gameObject.GetComponent<TaskManager>().wristMenu.SetActive(false);
            // }
            // this.transform.parent.gameObject.GetComponent<TaskManager>().wristMenu.SetActive(false);

            // this.transform.parent.gameObject.GetComponent<TaskManager>().TaskNumber++;
            this.transform.parent.gameObject.GetComponent<TaskManager>().TaskIncrement(Text);
        }
        else
        {
            // potShadow.GetComponent<Outline>().OutlineColor = Color.red;
            potShadow.GetComponent<Outline>().OutlineColor = Color.green;
            potShadow.GetComponent<Outline>().OutlineWidth = 4;
        }
    }
}
