[Return to Tutorials](https://github.com/ColumbiaCGUI/MercuryMessaging/wiki/Tutorials)


![Tutorial6Image](https://github.com/ColumbiaCGUI/MercuryMessaging/assets/87955067/166a828d-e2c4-40d7-8a59-26ffcb39fa18)

In this tutorial, we will show you how to design a [go/no-go task, a typical behavior experiment paradigm](https://link.springer.com/referenceworkentry/10.1007/978-3-319-47829-6_1598-1), with Mercury Messaging. The experiment is designed for VR, and we will use Mercury to send information about each trial to be saved. The experiment parameters such as the number of trials, trial duration, and inter-trial interval are exposed as public field in the game controller. They can be edited in the Unity editor.

# Prerequisites
* A Unity version supported by [MercuryMessage](https://github.com/ColumbiaCGUI/MercuryMessaging).
* A VR device that supports [OpenXR](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.10/manual/index.html).

To get started, clone the MercuryMessaging GitHub repository from [https://github.com/ColumbiaCGUI/MercuryMessaging](https://github.com/ColumbiaCGUI/MercuryMessaging). Then open it with Unity Editor. Navigate to MercuryMessaging/Tutorials/Tutorial6, and open the scene named `Tutorial6GoNogo`.

Now let's walk through the important game objects in the scene:
* GoNogoController: has the script named `GoNogoController.cs`. This is the main controller of the game logic. Its fields control how the experiment goes like the number of trials. You can refer to the tooltips of the fields for more information.
* GoNogoStimulusSphere: this is the stimulus in the experiment, it turns blue and yellow. When it is blue, a.k.a. the go trial, the player needs to press the primary button on their right-hand controller. When the stimulus is yellow, or the no-go trial, the player should not press the button.
* XR Interaction Hands Setup: this is the XR Interaction Toolkit setup for the player. It has a script named `GoNoGoButtonHandler.cs` attached. It uses the [XRI interaction mapping](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.0/manual/index.html) to listen to the button press event and sends the information to the `GoNogoController.cs`. 
* GoNoGoCanvas: this is the canvas that shows the trial number and the trial result. 
* DataRecorder: this is the data recorder that saves the trial information to a CSV file. It has a script named `DataRecorder.cs` attached. It listens to trial messages from `GoNogoController.cs` to get the trial information and saves it to a CSV file once a session is completed.

## How to Run the Experiment and Game Logic
The game logic is simple. After entering play mode, the user can press the `Start Gmae` button with their controller
to start a session, which consists of multiple trials. You can change the number of trials and other experiment parameters 
in the public field of `GoNoGoController`. The canvas will disappear once the game starts.

By default, in each trial, the stimulus stays on for 0.5 seconds (`trial duration`). Then it disappears,
then it waits 1 (`inter trial interval`) second before presenting the next stimulus. 
The player needs to press the primary button on their right-hand controller when the stimulus is blue (a go trial).
The player will earn one point if they press the button on a go trial, and lose one point if they press the button on a no-go trial.
The score will not change if the player does not press the button on a go trial.

Regardless of the trial type, once the button is pressed, it will immediately move to the next trial. 

Once all trials are completed, the game will show the score and save the trial information to a CSV file.
The canvas will reappear, and the player can press the `Start Game` button to start a new session.


## How the Data is Recorded -- Where Mercury Messaging Comes In

The trial data of each trial is produced the `GoNogoController.cs` script.
Here is the `GoNogoTrialData` class that holds the information of each trial:

```csharp
    public class GoNogoTrialData
    {
        public int TrialIndex;
        public int TrialType;
        public float ReactionTime;
        public int CurrentScore;
    
        public GoNogoTrialData(int trialIndex, int trialType, float reactionTime, int currentScore)
        {
            TrialIndex = trialIndex;
            TrialType = trialType;
            ReactionTime = reactionTime;
            currentScore = currentScore;
        }
    
        public override string ToString()
        {
            return $"{TrialIndex},{TrialType},{ReactionTime},{CurrentScore}";
        }
    }
```

The trial data is sent to the `DataRecorder.cs` script via Mercury Messaging.
The `DataRecorder.cs` script listens to the trial data message and keep them in a list.
Once a session completes, it saves the trial data to a CSV file.

## Further Readings
* [Tutorial VR: Basic networking with Mercury & Photon Fusion V2](https://github.com/ColumbiaCGUI/MercuryMessaging/wiki/Tutorial-VR:-Basic-networking-with-Mercury-&-Photon-Fusion-V2)
* [Tutorial 7: Task Management](https://github.com/ColumbiaCGUI/MercuryMessaging/wiki/Tutorial-7:-Task-Management)