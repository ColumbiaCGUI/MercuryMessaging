using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

using Fusion;
using MercuryMessaging;

using System;

using TMPro;

public class TaskManager : NetworkBehaviour
{
    public GameObject wristMenu;

    public TextMeshProUGUI taskText;

    private string Text = "Task1\n Light the Candles";
    // public List<GameObject> tasks;

    // public GameObject potShadow;
    MmRelayNode _relayNode;
    private NetworkRunner _runner;
    private NetworkObject _object;

    public int TaskNumber = 1;

    // Start is called before the first frame update
    void Start()
    {

        _relayNode = GetComponent<MmRelayNode>();
        _object = GetComponent<NetworkObject>();

        taskText.text = Text;

    }

    // Update is called once per frame
    void Update()
    {
        _runner = _object.Runner;
        // if(_runner==null)
        // {
        //     return;
        // }
        // if(_runner.IsRunning== false)
        // {
        //     return;
        // }

        // if(TaskNumber ==1)
        // {
        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         true,
        //         new MmMetadataBlock(((MmTag)(1)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );

        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         false,
        //         new MmMetadataBlock(((MmTag)(2)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );

        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         false,
        //         new MmMetadataBlock(((MmTag)(4)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );

        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         false,
        //         new MmMetadataBlock(((MmTag)(8)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );
        // }
        // else if(TaskNumber ==2)
        // {
        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         false,
        //         new MmMetadataBlock(((MmTag)(1)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );

        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         true,
        //         new MmMetadataBlock(((MmTag)(2)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );

        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         true,
        //         new MmMetadataBlock(((MmTag)(4)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );

        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         false,
        //         new MmMetadataBlock(((MmTag)(8)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );
        // }
        // else if(TaskNumber==3)
        // {
        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         false,
        //         new MmMetadataBlock(((MmTag)(1)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );

        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         false,
        //         new MmMetadataBlock(((MmTag)(2)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );

        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         false,
        //         new MmMetadataBlock(((MmTag)(4)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );

        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         true,
        //         new MmMetadataBlock(((MmTag)(8)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );
        // }
        // else
        // {
        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         false,
        //         new MmMetadataBlock(((MmTag)(1)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );

        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         false,
        //         new MmMetadataBlock(((MmTag)(2)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );

        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         false,
        //         new MmMetadataBlock(((MmTag)(4)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );

        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         false,
        //         new MmMetadataBlock(((MmTag)(8)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );

        //     _relayNode.MmInvoke(
        //         MmMethod.SetActive,
        //         false,
        //         new MmMetadataBlock(((MmTag)(16)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        //     );




        // }
    }

    public void TaskIncrement(string TaskText)
    {
        taskText.text = TaskText;

        if(wristMenu.activeSelf)
        {
            wristMenu.SetActive(false);
        }
        else
        {
            wristMenu.SetActive(true);
        }

        TaskNumber+=1;

        _relayNode.MmInvoke(
            MmMethod.SetActive,
            false,
            new MmMetadataBlock(((MmTag)(Math.Pow(2, TaskNumber-2))),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        );
        
        _relayNode.MmInvoke(
            MmMethod.SetActive,
            true,
            new MmMetadataBlock(((MmTag)(Math.Pow(2, TaskNumber-1))),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
        );

        
    }

}
