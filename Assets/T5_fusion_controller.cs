using UnityEngine;
using MercuryMessaging;

using Fusion;
// using Photon.Realtime;

public class T5_fusion_controller : NetworkBehaviour
{

    MmRelayNode _myRelayNode;
    bool active = true;

    private NetworkRunner networkRunner;
    private NetworkObject networkObject;

    public void Start()
    {
        _myRelayNode = GetComponent<MmRelayNode>();
        networkObject = GetComponent<NetworkObject>();
        // networkObject.RequestStateAuthority();
        // PhotonNetwork.ConnectUsingSettings();
    }

    void Update()
    {
        if(networkRunner==null)
        {
            return;
        }
        if(networkRunner.IsRunning== false)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) )
        {
            Debug.Log("Space key was pressed.");
            active = !active;
            _myRelayNode.MmInvoke(
                MmMethod.SetActive,
                active,
                new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
            );

            _myRelayNode.MmInvoke(
                MmMethod.SetActive,
                !active,
                new MmMetadataBlock(MmLevelFilter.Parent, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
            );
        }
        // else if (Input.GetKeyDown(KeyCode.Alpha1))
        // {
        //     _myRelayNode.MmInvoke(
        //         new T4_ColorMessage(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f),
        //         (MmMethod)T4_myMethods.UpdateColor,
        //         (MmMessageType)T4_myMsgTypes.Color,
        //         new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network))
        //     );
                
        // }
    }

    public void OnConnectedToServer()
    {
        // Debug.Log("Connected to server.");
        networkRunner =networkObject.Runner;
    }

    

}

