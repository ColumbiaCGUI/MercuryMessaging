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
    public GameObject sphere;
    public GameObject instructionCanvas;
    public TMPro.TextMeshProUGUI scoreText;

    public int numTrials;
    public float goPercentage;
    public float trialDuration;
    public float interTrialInterval;

    private string Text = "Task Completed!";

    [SerializeField] private Queue<int> goNoGoQueue = new Queue<int>();

    public bool isInTrial;
    public float trialTimer;
    [FormerlySerializedAs("curTrial")] public int curTrialType;
    public bool advanceTrial;
    
    public bool isButtonPressed;

    public int scores;
    public int trialCounter;
    
    public DataRecorder dataRecorder;
    
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
    void FixedNetworkUpdate()
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
                    sphere.GetComponent<BallFlash>().SetColor(new Color(0, 1, 0));
                }
                else
                {
                    sphere.GetComponent<BallFlash>().SetShow(true);
                    sphere.GetComponent<BallFlash>().SetColor(new Color(1, 0, 0));
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

        if(scores>=8)
        {
            this.transform.parent.gameObject.GetComponent<TaskManager>().TaskIncrement(Text);
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

        this.transform.parent.gameObject.GetComponent<TaskManager>().TaskIncrement(Text);
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
