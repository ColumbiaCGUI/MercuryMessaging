using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;

public class NetworkTransfer : MonoBehaviour
{
    private NetworkObject networkObject;
    // Start is called before the first frame update
    void Start()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    public void TransferAuthority()
    {
        if(networkObject != null && networkObject.Runner !=null)
        {
            networkObject.RequestStateAuthority();
        }
    }
}
