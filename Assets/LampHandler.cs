using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;
using MercuryMessaging;

public class LampHandler : NetworkBehaviour
{
    public GameObject fireParticle;
    MmRelayNode _relayNode;

    private NetworkRunner _runner;
    private NetworkObject _object;

    bool active = true;
    // Start is called before the first frame update
    void Start()
    {
        _relayNode = GetComponent<MmRelayNode>();
        _object = GetComponent<NetworkObject>();
    }

    // Update is called once per frame
    void Update()
    {
        _runner = _object.Runner;
        if(_runner==null)
        {
            // Debug.Log("NetworkRunner is null");
            return;
        }
        if(_runner.IsRunning== false)
        {
            // Debug.Log("NetworkRunner is not running");
            return;
        }

        if(Vector3.Distance(this.transform.position, fireParticle.transform.position) < 0.2f)
        {
            Debug.Log("Lamp is close to fire"+ active);
            // active = !active;
            _relayNode.MmInvoke(
                MmMethod.SetActive,
                active,
                new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
            );
        }
    }
}
