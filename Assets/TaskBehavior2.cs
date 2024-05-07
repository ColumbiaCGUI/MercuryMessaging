using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TaskBehavior2 : MonoBehaviour
{
    public TextMeshProUGUI taskText;

    private string Text= "Task2\n Find the treasure";
    // Start is called before the first frame update
    void Start()
    {
        taskText.text = Text;
    }

    // Update is called once per frame
    void Update()
    {
        taskText.text = Text;
    }
}
