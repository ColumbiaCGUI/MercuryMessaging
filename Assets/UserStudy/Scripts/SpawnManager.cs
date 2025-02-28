using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject pedestrianPrefab; 
    public GameObject[] carPrefabs; 
    public Vector3 spawnPos = new Vector3(-40.0f, 0.02400017f, 79.21f); 
    public List<Vector3> carSpawnPos; 
    public float startDelay = 2.0f;
    public float spawnInterval = 10.0f; 
    private int currentSpawnIndex = 0; 

    // Start is called before the first frame update
    void Start()
    {
        carSpawnPos.Add(new Vector3(-54.78f, 0.001151287f, 104)); 
        carSpawnPos.Add(new Vector3(-56.206f, 0.001151287f, 104)); 
        InvokeRepeating("SpawnPedestrian", startDelay, spawnInterval);
        InvokeRepeating("spawnCar", startDelay, spawnInterval);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnPedestrian() {
        Instantiate(pedestrianPrefab, spawnPos, pedestrianPrefab.transform.rotation);
        EventSystem.Instance.updateStatus("Population", 1); 
    }

    public void spawnCar() {
        int randomPrefabIndex = Random.Range(0, carPrefabs.Length);
        GameObject car = carPrefabs[randomPrefabIndex];
        Vector3 spawnPos = carSpawnPos[currentSpawnIndex]; 
        currentSpawnIndex = (currentSpawnIndex + 1) % 2;
        Instantiate(car, spawnPos, car.transform.rotation);
    }
}
