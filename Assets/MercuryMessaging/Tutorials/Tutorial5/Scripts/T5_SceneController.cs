using UnityEngine;
using MercuryMessaging;
using Photon.Pun;
using Photon.Realtime;
public class T5_SceneController : MonoBehaviourPunCallbacks
{
    MmRelayNode _myRelayNode;
    bool active = true;

    public void Start () 
    {
        _myRelayNode = GetComponent<MmRelayNode>();
        PhotonNetwork.ConnectUsingSettings();
    }

    void Update () 
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            active = !active;
            _myRelayNode.MmInvoke(
                MmMethod.SetActive, 
                active, 
                new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
            );
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        int randomRoomNumber = Random.Range(0, 1000);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        roomOptions.MaxPlayers = (byte)5; // Or whatever number you want

        PhotonNetwork.CreateRoom("Room" + randomRoomNumber, roomOptions);
    }
}
