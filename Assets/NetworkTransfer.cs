using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;

public class NetworkTransfer : NetworkBehaviour
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
            if(networkObject.HasStateAuthority==false)
            {
                networkObject.RequestStateAuthority();
            }
        }
    }

    public void ResetAuthority()
    {
        if(networkObject != null && networkObject.Runner !=null)
        {
            if(networkObject.HasStateAuthority==true)
            {
                networkObject.ReleaseStateAuthority();
            }
            // networkObject.ReleaseStateAuthority();
        }
    }
}
