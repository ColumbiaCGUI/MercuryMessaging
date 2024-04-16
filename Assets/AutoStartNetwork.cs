// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Fusion;

// public class AutoStartNetwork : MonoBehaviour
// {

//     private NetworkRunner runner;

//     // Start is called before the first frame update
//     void Awake()
//     {
//         runner = GetComponent<NetworkRunner>();

//         if (runner == null)
//         {
//             runner = gameObject.AddComponent<NetworkRunner>();
//         }

//         StartCoroutine(StartNetworkRunner());

//     }

//     // // Update is called once per frame
//     // void Update()
//     // {
        
//     // }
//     private IEnumerator StartNetworkRunner()
//     {
//         var startTask = runner.StartGame(new StartGameArgs{
//             GameMode = GameMode.Shared,
//             SessionName = "MySession"
//         });

//         yield return new WaitUntil(() => startTask. IsCompleted);

//         if(startTask.IsCompletedSuccessfully)
//         {
//             Debug.Log("Network started");
//         }
//         else
//         {
//             Debug.LogError("Network failed to start");
//         }
//     }
// }
