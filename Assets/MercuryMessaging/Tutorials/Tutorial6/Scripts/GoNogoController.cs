using System.Collections;
using System.Collections.Generic;
using Fusion;
using MercuryMessaging;
using UnityEngine;
using UnityEngine.Serialization;

public enum GoNogoMethods
{
    RecordTrial = 100
}

public enum GoNogoMsgTypes
{
    Trial = 1100
}

public class GoNogoController : MmBaseResponder
{   
    [Header("In-game Objects, do not change")]
    public GameObject sphere;
    public GameObject instructionCanvas;
    public TMPro.TextMeshProUGUI scoreText;
    public DataRecorder dataRecorder;

    [Header("Parameters for the go/no-go experiment, you can edit in Unity Editor")]
    [Tooltip("Number of trials (i.e., sphere flashes) in the experiment")]
    public int numTrials;
    [Tooltip("Percentage of 'go' trials in the experiment")]
    public float goPercentage;
    [Tooltip("Duration of how long the stimulus stays on screen")]
    public float trialDuration;
    [Tooltip("Duration of the interval between trials")]
    public float interTrialInterval;

    private string Text = "Task Completed!";

    private Queue<int> goNoGoQueue = new Queue<int>();
    
    [Header("In-game Variables that change during a play, they are visible for debugging, do not change them manually unless you know what you are doing")]
    [SerializeField] private bool isInTrial;
    [SerializeField] private float trialTimer;
    [SerializeField] private int curTrialType;
    [SerializeField] private bool advanceTrial;
    [SerializeField] private bool isButtonPressed;
    [SerializeField] private int scores;
    [SerializeField] private int trialCounter;
    
    public override void MmInvoke(MmMessage message)
    {
        var type = message.MmMethod;

        switch (type)
        {
            case ((MmMethod) GoNogoMethods.RecordTrial):
                break;
            default:
                base.MmInvoke(message);
                break;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (goNoGoQueue.Count == 0 && isInTrial)
        {
            ResetGame();
            dataRecorder.SaveData();
        }
        if (isInTrial)
        {
            trialTimer += Time.deltaTime;

            if (advanceTrial)
            {
                curTrialType = goNoGoQueue.Dequeue();
                advanceTrial = false;
            }

            if (trialTimer < trialDuration)
            {
                if (curTrialType == 1)
                {
                    sphere.GetComponent<BallFlash>().SetShow(true);
                    sphere.GetComponent<BallFlash>().SetColor(new Color(0, 1, 1));
                }
                else
                {
                    sphere.GetComponent<BallFlash>().SetShow(true);
                    sphere.GetComponent<BallFlash>().SetColor(new Color(1, 1, 0));
                }
            }
            else if (trialTimer <= trialDuration + interTrialInterval)
            {
                sphere.GetComponent<BallFlash>().SetShow(false);
            }
            else
            {
                EndTrial();
            }
            
            // if button pressed any time during a trial, advance to the next
            if (trialTimer <= trialDuration + interTrialInterval && isButtonPressed)
            {
                // this if block is triggered when the button is pressed during the trial
                // TODO you can record the button press time here, just use trialTimer
                
                if (curTrialType == 1)
                {
                    scores++;
                }
                else
                {
                    scores--;
                }
                
            EndTrial();
            }
        }

        if (transform.parent != null)
        {
            if(scores>=8)
            {
                this.transform.parent.gameObject.GetComponent<TaskManager>().taskText.text = Text;

                if(this.transform.parent.gameObject.GetComponent<TaskManager>().wristMenu.activeSelf)
                {
                    this.transform.parent.gameObject.GetComponent<TaskManager>().wristMenu.SetActive(false);
                }
                else
                {
                    this.transform.parent.gameObject.GetComponent<TaskManager>().wristMenu.SetActive(true);
                }
            
                this.transform.parent.gameObject.GetComponent<TaskManager>().TaskNumber+=1;
                // Debug.Log("TaskNumber: "+TaskNumber);
                this.transform.parent.gameObject.GetComponent<TaskManager>().TaskIncrement();
            }
        }

    }

    private void EndTrial()
    {
        if (!isButtonPressed)
        {
            trialTimer = -1f;
        }
        var trialData = new GoNogoTrialData(trialCounter, curTrialType, trialTimer, scores);
        GetRelayNode().MmInvoke( 
            new GoNoGoMessage(trialData,
                (MmMethod)GoNogoMethods.RecordTrial, 
                (MmMessageType)GoNogoMsgTypes.Trial, 
                new MmMetadataBlock(MmLevelFilter.Child)));
        
        trialTimer = 0;
        advanceTrial = true;
        isButtonPressed = false;
        sphere.GetComponent<BallFlash>().SetShow(false);
        
        if (transform.parent != null)
        {
            transform.parent.gameObject.GetComponent<TaskManager>().TaskIncrement();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Make sure the player GameObject has the tag "Player"
        {
            instructionCanvas.SetActive(true);
        }
    }

    public void StartGame()
    {
        // create a list of random permutation of 0s and 1s based on the goPercentage
        instructionCanvas.SetActive(false);
        goNoGoQueue = new Queue<int>();
        int numGoTrials = (int)(numTrials * goPercentage);
        for (int i = 0; i < numGoTrials; i++)
        {
            goNoGoQueue.Enqueue(1);
        }
        for (int i = 0; i < numTrials - numGoTrials; i++)
        {
            goNoGoQueue.Enqueue(0);
        }
        goNoGoQueue = Utils.ShuffleQueue(goNoGoQueue);
        isInTrial = true;
    }

    public void ResetGame()
    {
        // TODO export the trial results here
        
        // display the score
        scoreText.text = "Score: " + scores;
        
        instructionCanvas.SetActive(true);
        sphere.GetComponent<BallFlash>().SetShow(false);
        goNoGoQueue.Clear();
        isInTrial = false;
        trialTimer = 0;
        isButtonPressed = false;
        scores = 0;
        trialCounter = 0;
    }
}
