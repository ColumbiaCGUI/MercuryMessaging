using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskEnding : MonoBehaviour
{
    public GameObject FinalDoor;

    // private string Text = "Task Completed!";
    // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // Update is called once per frame
    void Update()
    {
        FinalDoor.SetActive(false);
        // this.transform.parent.gameObject.GetComponent<TaskManager>().taskText.text = Text;
        this.transform.parent.gameObject.GetComponent<TaskManager>().wristMenu.SetActive(true);
    }
}
