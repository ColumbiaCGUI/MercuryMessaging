using UnityEngine;
using MercuryMessaging;

using Fusion;
// using Photon.Realtime;

public class T5_fusion_controller : NetworkBehaviour
{

    MmRelayNode _myRelayNode;
    bool active = true;


    public void Start()
    {
        _myRelayNode = GetComponent<MmRelayNode>();
        // PhotonNetwork.ConnectUsingSettings();
    }

    void Update()
    {
        if (HasStateAuthority == false)
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
        }
    }

    

}

