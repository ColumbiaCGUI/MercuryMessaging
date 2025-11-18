using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskEnding : MonoBehaviour
{
    public GameObject FinalDoor;
    
    public AudioClip clip; // Assign your AudioClip here in the Inspector

    private AudioSource audioSource;

    // private string Text = "Task Completed!";
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        FinalDoor.SetActive(false);
        // this.transform.parent.gameObject.GetComponent<TaskManager>().taskText.text = Text;
        this.transform.parent.gameObject.GetComponent<TaskManager>().wristMenu.SetActive(true);
        audioSource.PlayOneShot(clip);
    }
}
