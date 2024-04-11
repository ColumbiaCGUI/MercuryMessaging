// using UnityEngine;
// using System;


// #if PHOTON_AVAILABLE
// using ExitGames.Client.Photon;
// using Photon.Realtime;
// using Photon.Fusion;

// namespace MercuryMessaging
// {
//     [RequireComponent(typeof(NetworkObject))]
//     public class MmNetworkResponderPhotonFusion : MmNetworkResponder
//     {
//         private NetworkObject _networkObject;
//         private NetworkRunner _networkRunner;

//         public override void Awake()
//         {
//             base.Awake();
//             // _networkObject = GetComponent<NetworkObject>();
//         }

//         public override void Start()
//         {
//             base.Start();
//             _networkObject = GetComponent<NetworkObject>();
//             _networkRunner = NetworkRunner.Instance;
//         }

//         private void OnEnable()
//         {

//         }

//         private void OnDisable()
//         {

//         }

//         public override bool IsActiveAndEnabled{ get { return _networkObject.IsSpawned && runner != null && runner.IsRunning; } }

//         public override bool OnServer{ get { return runner!=null && runner.IsServer; } }

//         public override bool OnClient{ get { return runner!=null && runner.IsClient; } }




//     }
// }

// #endif