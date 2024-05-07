using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    //creating a singleton
    public static NetworkManager Instance { get; private set; }

    [SerializeField]
    private GameObject _runnerPrefab;

    public GameObject canvas;

    public GameObject handCanvas;

    public GameObject SetUpPosition;

    public NetworkRunner Runner { get; private set; }

    private bool GameStarted = false;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        // fixing the server to a perticular region
        Fusion.Photon.Realtime.PhotonAppSettings.Global.AppSettings.FixedRegion = "asia";
        GameStarted = false;

    }

    private void Update()
    {
        if(!GameStarted)
        {
            GameStarted = true;
            StartCoroutine(StartGame());
        }
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(3);
        JoinSession("1234");
    }

    public async void CreateSession(string roomCode)
    {
        //Create Runner
        CreateRunner();
        //Load Scene
        // await LoadScene();
        //ConnectSession
        await Connect(roomCode);
    }

    public async void JoinSession(string roomCode)
    {
        //Create Runner
        CreateRunner();
        //Load Scene
        // await LoadScene();
        //ConnectSession
        await Connect(roomCode);
        
        canvas.SetActive(false);
    }

    public void CreateRunner()
    {
        Runner =Instantiate(_runnerPrefab, transform).GetComponent<NetworkRunner>();
        Runner.AddCallbacks(this);

        
    }

    public async Task LoadScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);

        while (!asyncLoad.isDone)
        {
            await Task.Yield();
        }
    }

    private async Task Connect(string SessionName)
    {
        var args = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = SessionName,
            SceneManager = GetComponent<NetworkSceneManagerDefault>(),
            Scene = SceneRef.FromIndex(0)

        };
        await Runner.StartGame(args);

        // canvas.SetActive(false);

    }

    #region INetworkRunnerCallbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("<<<<<<<< A new player joined to the session >>>>>>>");
        Debug.Log("<<<<<<< IsMasterClient >>>>>>>>" + player.IsMasterClient);
        Debug.Log("<<<<<<< PlayerID >>>>>>>>" + player.PlayerId);
        Debug.Log("<<<<<<< IsRealPlayer >>>>>>>>" + player.IsRealPlayer);

        if(runner.LocalPlayer.RawEncoded==1)
        {
            handCanvas.SetActive(true);
            Vector3 position = SetUpPosition.transform.position;
            position.x -= 5f;
            SetUpPosition.transform.position= position;
        }
        else
        {
            handCanvas.SetActive(false);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("<<<<<<<< A player left the session >>>>>>>");
        Debug.Log("<<<<<<< IsMasterClient >>>>>>>>" + player.IsMasterClient);
        Debug.Log("<<<<<<< PlayerID >>>>>>>>" + player.PlayerId);
        Debug.Log("<<<<<<< IsRealPlayer >>>>>>>>" + player.IsRealPlayer);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("<<<<<<< Runner Shutdown >>>>>>>>");

    }
    #endregion

    #region INetworkRunnerCallbacks (Unused)
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


    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
    #endregion

}
