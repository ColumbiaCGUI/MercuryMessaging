using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;
using MercuryMessaging;

public class LampHandler : NetworkBehaviour
{
    public GameObject fireParticle1;

    public GameObject fireParticle2;

    public GameObject fireParticle3;

    public GameObject fireParticle4;

    public Transform fireTip;

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

        if(Vector3.Distance(fireTip.position, fireParticle1.transform.position) < 0.2f)
        {
            // Debug.Log("Lamp is close to fire"+ active);
            // active = !active;
            _relayNode.MmInvoke(
                MmMethod.SetActive,
                active,
                new MmMetadataBlock(((MmTag)(1)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
            );
        }
        else if(Vector3.Distance(fireTip.position, fireParticle2.transform.position) < 0.2f)
        {
            // Debug.Log("Lamp is close to fire"+ active);
            // active = !active;
            _relayNode.MmInvoke(
                MmMethod.SetActive,
                active,
                new MmMetadataBlock(((MmTag)(2)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
            );
        }
        else if (Vector3.Distance(fireTip.position, fireParticle3.transform.position) < 0.2f)
        {
            // Debug.Log("Lamp is close to fire"+ active);
            // active = !active;
            _relayNode.MmInvoke(
                MmMethod.SetActive,
                active,
                new MmMetadataBlock(((MmTag)(4)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
            );
        }
        else if (Vector3.Distance(fireTip.position, fireParticle4.transform.position) < 0.2f)
        {
            // Debug.Log("Lamp is close to fire"+ active);
            // active = !active;
            _relayNode.MmInvoke(
                MmMethod.SetActive,
                active,
                new MmMetadataBlock(((MmTag)(8)),MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
            );
        }
    }
}
