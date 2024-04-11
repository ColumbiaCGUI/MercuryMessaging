using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MercuryMessaging;

public class T2_SceneController : MonoBehaviour
{
    MmRelayNode _myRelayNode;
    bool activeUp = true;
    bool activeDown = true;

    // Start is called before the first frame update
    public void Start()
    {
        _myRelayNode  = GetComponent<MmRelayNode>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            activeUp = !activeUp;
            _myRelayNode.MmInvoke(MmMethod.SetActive, activeUp, new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All));
        }
        else if(Input.GetKeyDown(KeyCode.Return))
        {
            activeDown = !activeDown;
            _myRelayNode.MmInvoke(MmMethod.SetActive, activeDown, new MmMetadataBlock(MmLevelFilter.Parent, MmActiveFilter.All));
        }
    }
}
