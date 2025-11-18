using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;
public class T5_PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Runner.Spawn(PlayerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }
}
