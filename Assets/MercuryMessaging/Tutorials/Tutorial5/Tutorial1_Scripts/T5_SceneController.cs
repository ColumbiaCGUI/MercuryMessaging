using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MercuryMessaging;

public class T5_SceneController : MonoBehaviour
{
    MmRelayNode _myRelayNode;
    bool active = true;

    public void Start () 
    {
        _myRelayNode = GetComponent<MmRelayNode>();
    }

    void Update () 
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            active = !active;
            _myRelayNode.MmInvoke(MmMethod.SetActive, active,
                new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All));
        }
    }
}
