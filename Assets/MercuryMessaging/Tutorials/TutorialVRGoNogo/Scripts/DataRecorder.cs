using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MercuryMessaging;
using UnityEngine;



public class DataRecorder : MmBaseResponder
{
    
    List<GoNogoTrialData> trialDataList = new List<GoNogoTrialData>();
    
    public string dataSavePath = "Assets";
    
    public override void MmInvoke(MmMessage message)
    {
        var type = message.MmMethod;

        switch (type)
        {
            case ((MmMethod) GoNogoMethods.RecordTrial):
                GoNogoTrialData trialData = ((GoNoGoMessage) message).trialData; 
                RecordTrial(trialData);
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
        
    }
    
    void RecordTrial(GoNogoTrialData trialData)
    {
        trialDataList.Add(trialData);
    }

    public void SaveData()
    {
        Debug.Log("Saving data");
        
        StringBuilder sb = new StringBuilder();

        // Optional: Add header line
        sb.AppendLine("TrialIndex,TrialType,ReactionTime");

        // Add data lines
        foreach (GoNogoTrialData data in trialDataList)
        {
            sb.AppendLine(data.ToString());
        }

        // File path
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        string filePath = Path.Combine(Application.persistentDataPath, $"ExperimentData_{timestamp}.csv");

        // Write to file
        File.WriteAllText(filePath, sb.ToString());

        Debug.Log($"Data saved to {filePath}");
    }
}
