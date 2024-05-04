using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardwareRig : MonoBehaviour, INetworkRunnerCallbacks
{
    public Transform playerTransform;

    public Transform headTransform;

    public Transform leftHandTransform;

    public Transform rightHandTransform;

    void Start()
    {
        NetworkManager.Instance.Runner.AddCallbacks(this);
    }


    #region INetworkRunnerCallbacks
    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        RigState xrRigState = new RigState();

        xrRigState.HeadsetPosition = headTransform.position;
        xrRigState.HeadsetRotation = headTransform.rotation;

        xrRigState.PlayerPosition = playerTransform.position;
        xrRigState.PlayerRotation = playerTransform.rotation;

        xrRigState.LeftHandPosition = leftHandTransform.position;
        xrRigState.LeftHandRotation = leftHandTransform.rotation;

        xrRigState.RightHandPosition = rightHandTransform.position;
        xrRigState.RightHandRotation = rightHandTransform.rotation;

        input.Set(xrRigState);
    }
    #endregion

    #region Unused INetworkRunnerCallbacks
    public void OnConnectedToServer(NetworkRunner runner)
    {
 
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
 
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
 
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
 
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
 
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
 
    }


    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
 
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
 
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
 
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
 
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
 
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
 
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
 
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
 
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
 
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
 
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
 
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
 
    }
    #endregion

}

public struct RigState : INetworkInput
{
    public Vector3 PlayerPosition;
    public Quaternion PlayerRotation;

    public Vector3 HeadsetPosition;
    public Quaternion HeadsetRotation;

    public Vector3 LeftHandPosition;
    public Quaternion LeftHandRotation;

    public Vector3 RightHandPosition;
    public Quaternion RightHandRotation;
}
