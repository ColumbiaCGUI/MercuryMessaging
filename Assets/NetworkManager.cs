using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;

[RequireComponent(typeof(NetworkEvents))]
public class NetworkManager : MonoBehaviour
{
    
    public void ServerConnected()
    {
        Debug.Log("Server connected");
    }

    public void ServerDisconnected()
    {
        Debug.Log("Server disconnected");
    }

}
