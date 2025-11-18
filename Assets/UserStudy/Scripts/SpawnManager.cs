using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject pedestrianPrefab; 
    public GameObject[] carPrefabs; 
    public Vector3 spawnPosDirection2;
    public Vector3 spawnPosDirection1;
    public List<Vector3> carSpawnPos; 
    private Vector3 carSpawnPosDirection2; 
    public float startDelay = 2.0f;
    public float spawnInterval = 10.0f; 
    private int currentSpawnIndex = 0; 
    public GameObject CarRoot; 
    public GameObject ZombieRoot; 

    // Start is called before the first frame update
    void Start()
    {
        spawnPosDirection2 = new Vector3(StreetInfo.Instance.humanSpawnLocationX, 0.02400017f, 79.21f); 
        spawnPosDirection1 = new Vector3(-49.929f, 0.02399635f, StreetInfo.Instance.humanSpawnLocationZ);
        carSpawnPos.Add(new Vector3(-54.78f, 0.001151287f, 104)); 
        carSpawnPos.Add(new Vector3(-56.206f, 0.001151287f, 104));
        carSpawnPosDirection2 = new Vector3(StreetInfo.Instance.carSpawnLocationX, 0.0005757086f, 76.94f); 
        InvokeRepeating("SpawnPedestrianDirection2", startDelay, spawnInterval);
        InvokeRepeating("SpawnPedestrianDirection1", startDelay + 2.0f, spawnInterval);
        InvokeRepeating("SpawnCarDirection1", startDelay, spawnInterval);
        InvokeRepeating("SpawnCarDirection2", startDelay + 2.0f, spawnInterval);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnPedestrianDirection2() {
        GameObject instance = Instantiate(pedestrianPrefab, spawnPosDirection2, Quaternion.Euler(0, -90, 0));
        Pedestrian pedestrian = instance.GetComponent<Pedestrian>();
        pedestrian.direction1 = false; 
        TrafficEventManager.Instance.updateStatus("Population", 1); 
        instance.transform.SetParent(ZombieRoot.transform, true); 
    }

    public void SpawnPedestrianDirection1() {
        GameObject instance = Instantiate(pedestrianPrefab, spawnPosDirection1, Quaternion.Euler(0, -180, 0));
        Pedestrian pedestrian = instance.GetComponent<Pedestrian>();
        pedestrian.direction1 = true; 
        TrafficEventManager.Instance.updateStatus("Population", 1); 
        instance.transform.SetParent(ZombieRoot.transform, true); 
    }

    public void SpawnCarDirection1() {
        int randomPrefabIndex = Random.Range(0, carPrefabs.Length);
        GameObject car = carPrefabs[randomPrefabIndex];
        Vector3 spawnPos = carSpawnPos[currentSpawnIndex]; 
        currentSpawnIndex = (currentSpawnIndex + 1) % 2;
        GameObject instance = Instantiate(car, spawnPos, car.transform.rotation);
        CarController carController = instance.GetComponent<CarController>();
        carController.direction1 = true;
        instance.transform.SetParent(CarRoot.transform, true); 
    }

    public void SpawnCarDirection2() {
        int randomPrefabIndex = Random.Range(0, carPrefabs.Length);
        GameObject car = carPrefabs[randomPrefabIndex];
        Vector3 spawnPos = carSpawnPosDirection2;
        GameObject instance = Instantiate(car, spawnPos, Quaternion.Euler(0, -90, 0));
        CarController carController = instance.GetComponent<CarController>();
        carController.direction1 = false;
        instance.transform.SetParent(CarRoot.transform, true); 
    }
}
