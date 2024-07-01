using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpwanPlayer : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField]
    private NetworkPrefabRef playerPrefab;

    // Dictionary of spawned user prefabs, to destroy them on disconnection
    private Dictionary<PlayerRef, NetworkObject> _spawnedUsers = new Dictionary<PlayerRef, NetworkObject>();
    
    void Start()
    {
        NetworkManager.Instance.Runner.AddCallbacks(this);
    }

    #region INetworkRunnerCallbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer && playerPrefab != null)
        {
            //Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(1, 5), 0.5f, UnityEngine.Random.Range(1, 5));
            // if(player.PlayerId == 1)
            // {
            //     playerPrefab.
            // }

            NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
            
            // networkPlayerObject.gameObject.GetComponent<SetOutline>().SetOutlineColor(Color.green);
            networkPlayerObject.gameObject.GetComponent<SetOutline>().SetCullingMusk();
            // gameObject.layer = LayerMask.NameToLayer("MyLayer");
            
            // Keep track of the player avatars so we can remove it when they disconnect
            _spawnedUsers.Add(player, networkPlayerObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedUsers.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedUsers.Remove(player);
        }
    }
    #endregion

    #region Unsed INetworkRunnerCallbacks
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

    public void OnInput(NetworkRunner runner, NetworkInput input)
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
