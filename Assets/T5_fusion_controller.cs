using UnityEngine;
using MercuryMessaging;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

using Fusion;
// using Photon.Realtime;

public class T5_fusion_controller : NetworkBehaviour
{

    MmRelayNode _myRelayNode;
    bool active = true;

    bool candleActive = true;

    private NetworkRunner networkRunner;
    private NetworkObject networkObject;


    [SerializeField]
    // set the XRI default input action to be the player input
    public InputActionAsset playerInput;

    private InputAction turnOffAction;

    // private CharacterController _controller;  

    // public float PlayerSpeed = 2f;


    public void Start()
    {
        _myRelayNode = GetComponent<MmRelayNode>();
        networkObject = GetComponent<NetworkObject>();
        turnOffAction=playerInput.FindActionMap("XRI RightHand Interaction").FindAction("RightPrimary");
        turnOffAction.Enable();
        // _controller = GetComponent<CharacterController>();
        // networkObject.RequestStateAuthority();
        // PhotonNetwork.ConnectUsingSettings();
    }

    void Update()
    {

        networkRunner = networkObject.Runner;
        if(networkRunner==null)
        {
            // Debug.Log("NetworkRunner is null");
            return;
        }
        if(networkRunner.IsRunning== false)
        {
            // Debug.Log("NetworkRunner is not running");
            return;
        }

        if (turnOffAction.triggered)
        {
            // Debug.Log("Space key was pressed.");
            active = !active;
            _myRelayNode.MmInvoke(
                MmMethod.SetActive,
                active,
                new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
            );

            _myRelayNode.MmInvoke(
                MmMethod.SetActive,
                !active,
                new MmMetadataBlock(MmLevelFilter.Parent, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network)
            );
        }
    }

    

}

