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
        if(networkRunner==null)
        {
            return;
        }
        if(networkRunner.IsRunning== false)
        {
            return;
        }

        if (turnOffAction.triggered)
        {
            Debug.Log("Space key was pressed.");
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
        // else if (Input.GetKeyDown(KeyCode.Alpha1))
        // {
        //     _myRelayNode.MmInvoke(
        //         new T4_ColorMessage(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f),
        //         (MmMethod)T4_myMethods.UpdateColor,
        //         (MmMessageType)T4_myMsgTypes.Color,
        //         new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Network))
        //     );
                
        // }
    }

    public void OnConnectedToServer()
    {
        // Debug.Log("Connected to server.");
        networkRunner =networkObject.Runner;
    }

    // public override void FixedUpdateNetwork()
    // {
    //     Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Runner.DeltaTime * PlayerSpeed;
    //     _controller.Move(move);
    // }

    

}

