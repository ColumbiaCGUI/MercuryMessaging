using System;
using MercuryMessaging.Task;

// Suppress CS0618: IMmSerializable is obsolete - kept for tutorial compatibility
#pragma warning disable CS0618

/// <summary>
/// Tutorial 9: Custom task info class implementing IMmTaskInfo.
/// Defines the data for each trial/task in an experiment.
///
/// Extends the base IMmTaskInfo interface with experiment-specific fields
/// like StimulusType, TargetValue, TrialNumber, Condition, TimeLimit.
/// </summary>
[Serializable]
public class T9_MyTaskInfo : IMmTaskInfo, IMmSerializable
{
    #region IMmTaskInfo Required Properties

    /// <summary>
    /// Record ID
    /// </summary>
    public int RecordId { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Sequence ID within user
    /// </summary>
    public int UserSequence { get; set; }

    /// <summary>
    /// Task ID
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// Name of the task (e.g., "Instructions", "Practice", "Trial", "Complete")
    /// </summary>
    public string TaskName { get; set; }

    /// <summary>
    /// Flag to skip recording data for this task
    /// </summary>
    public bool DoNotRecordData { get; set; }

    #endregion

    #region Custom Experiment Fields

    /// <summary>
    /// Type of stimulus (e.g., "visual", "auditory")
    /// </summary>
    public string StimulusType { get; set; }

    /// <summary>
    /// Target value for this trial
    /// </summary>
    public float TargetValue { get; set; }

    /// <summary>
    /// Trial number within the experiment
    /// </summary>
    public int TrialNumber { get; set; }

    /// <summary>
    /// Experimental condition (e.g., "A", "B", "practice")
    /// </summary>
    public string Condition { get; set; }

    /// <summary>
    /// Time limit for response in seconds
    /// </summary>
    public float TimeLimit { get; set; }

    /// <summary>
    /// Instructions text for this task
    /// </summary>
    public string Instructions { get; set; }

    /// <summary>
    /// Task index for ordering
    /// </summary>
    public int TaskIndex { get; set; }

    /// <summary>
    /// Whether task is complete
    /// </summary>
    public bool IsComplete { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Default constructor with sensible defaults
    /// </summary>
    public T9_MyTaskInfo()
    {
        RecordId = 0;
        UserId = 0;
        UserSequence = 0;
        TaskId = 0;
        TaskName = "Task";
        DoNotRecordData = false;
        StimulusType = "visual";
        TargetValue = 0.5f;
        TrialNumber = 1;
        Condition = "A";
        TimeLimit = 5f;
        Instructions = "";
        TaskIndex = 0;
        IsComplete = false;
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public T9_MyTaskInfo(T9_MyTaskInfo orig)
    {
        RecordId = orig.RecordId;
        UserId = orig.UserId;
        UserSequence = orig.UserSequence;
        TaskId = orig.TaskId;
        TaskName = orig.TaskName;
        DoNotRecordData = orig.DoNotRecordData;
        StimulusType = orig.StimulusType;
        TargetValue = orig.TargetValue;
        TrialNumber = orig.TrialNumber;
        Condition = orig.Condition;
        TimeLimit = orig.TimeLimit;
        Instructions = orig.Instructions;
        TaskIndex = orig.TaskIndex;
        IsComplete = orig.IsComplete;
    }

    #endregion

    #region IMmTaskInfo Methods

    /// <summary>
    /// Parse task info from CSV string.
    /// </summary>
    public int Parse(string str)
    {
        var words = str.Split(',');
        int i = 0;

        // Base IMmTaskInfo fields
        RecordId = int.Parse(words[i++]);
        UserId = int.Parse(words[i++]);
        UserSequence = int.Parse(words[i++]);
        TaskId = int.Parse(words[i++]);
        DoNotRecordData = bool.Parse(words[i++]);
        TaskName = words[i++];

        // Custom fields
        if (words.Length > i) StimulusType = words[i++];
        if (words.Length > i) TargetValue = float.Parse(words[i++]);
        if (words.Length > i) TrialNumber = int.Parse(words[i++]);
        if (words.Length > i) Condition = words[i++];
        if (words.Length > i) TimeLimit = float.Parse(words[i++]);
        if (words.Length > i) Instructions = words[i++];
        if (words.Length > i) TaskIndex = int.Parse(words[i++]);
        if (words.Length > i) IsComplete = bool.Parse(words[i++]);

        return i;
    }

    /// <summary>
    /// CSV headers for file output.
    /// </summary>
    public string Headers()
    {
        return "RecordId,UserId,UserSequence,TaskId,DoNotRecordData,TaskName," +
               "StimulusType,TargetValue,TrialNumber,Condition,TimeLimit,Instructions,TaskIndex,IsComplete";
    }

    /// <summary>
    /// String representation for debugging and CSV output.
    /// </summary>
    public override string ToString()
    {
        return $"Task {TaskIndex}: {TaskName} (Trial {TrialNumber}, {Condition})";
    }

    #endregion

    #region IMmSerializable Methods

    /// <summary>
    /// Create a copy of this task info.
    /// </summary>
    public IMmSerializable Copy()
    {
        return new T9_MyTaskInfo(this);
    }

    /// <summary>
    /// Serialize to object array for network transmission.
    /// </summary>
    public object[] Serialize()
    {
        return new object[]
        {
            RecordId,
            UserId,
            UserSequence,
            TaskId,
            DoNotRecordData,
            TaskName,
            StimulusType,
            TargetValue,
            TrialNumber,
            Condition,
            TimeLimit,
            Instructions,
            TaskIndex,
            IsComplete
        };
    }

    /// <summary>
    /// Deserialize from object array.
    /// </summary>
    public int Deserialize(object[] data, int index)
    {
        RecordId = Convert.ToInt32(data[index++]);
        UserId = Convert.ToInt32(data[index++]);
        UserSequence = Convert.ToInt32(data[index++]);
        TaskId = Convert.ToInt32(data[index++]);
        DoNotRecordData = Convert.ToBoolean(data[index++]);
        TaskName = (string)data[index++];
        StimulusType = (string)data[index++];
        TargetValue = Convert.ToSingle(data[index++]);
        TrialNumber = Convert.ToInt32(data[index++]);
        Condition = (string)data[index++];
        TimeLimit = Convert.ToSingle(data[index++]);
        Instructions = (string)data[index++];
        TaskIndex = Convert.ToInt32(data[index++]);
        IsComplete = Convert.ToBoolean(data[index++]);
        return index;
    }

    #endregion
}
