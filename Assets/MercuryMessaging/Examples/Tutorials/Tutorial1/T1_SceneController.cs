using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MercuryMessaging;

public class T1_SceneController : MonoBehaviour
{
    MmRelayNode _myRelayNode;
    bool active = true;
    // Start is called before the first frame update
    public void Start () 
    {
        _myRelayNode = GetComponent<MmRelayNode>();
    }

    // Update is called once per frame
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
