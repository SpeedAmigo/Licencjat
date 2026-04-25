using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

public class EggSpawner : NetworkBehaviour
{
    public static event Action<NetworkObject> EggSpawned;
    
    [Header("Assigned value to spawn")]
    public float amountToSpawn;
    public float spawnedValue;
    
    [Header("Spawnable objects list")]
    [SerializeField] private List<SpawnAbleObject> spawnAbleObjects;

    private List<DogItemOfInterest> _eggsInRadius = new();
    
    public void SpawnObject()
    {
        var pickedObject = PickObjectToSpawn();
        
        var pooledObject = NetworkManager.GetPooledInstantiated(pickedObject.prefab,true);
        
        pooledObject.transform.position = transform.position;
        pooledObject.transform.rotation = transform.rotation;
        
        Spawn(pooledObject);
        spawnedValue++;
        
        EggSpawned?.Invoke(pooledObject);
        
        if (spawnedValue < amountToSpawn && _eggsInRadius.Count < amountToSpawn)
        {
            SpawnObject();
        }
    }

    private SpawnAbleObject PickObjectToSpawn()
    {
        int currentDay = 1;
        
        List<SpawnAbleObject> availableObjects = new List<SpawnAbleObject>();

        foreach (var obj in spawnAbleObjects)
        {
            if (obj.availableSinceDay <= currentDay && obj.spawnChance > 0f)
            {
                availableObjects.Add(obj);
            }
        }

        if (availableObjects.Count == 0)
        {
            Debug.LogWarning("ObjectSpawner: No Available Objects Found");
            return null;
        }
        
        float totalChance = 0f;
        foreach (var obj in availableObjects)
        {
            totalChance += obj.spawnChance;
        }
        
        float roll = Random.Range(0f, totalChance);

        float cumulative = 0f;
        foreach (var obj in availableObjects)
        {
            cumulative += obj.spawnChance;
            if (roll <= cumulative)
            {
                return obj;
            }
        }
        
        return availableObjects[^1];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<DogItemOfInterest>(out var dogItemOfInterest))
        {
            if (_eggsInRadius.Contains(dogItemOfInterest)) return;
            
            _eggsInRadius.Add(dogItemOfInterest);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<DogItemOfInterest>(out var dogItemOfInterest))
        {
            if (!_eggsInRadius.Contains(dogItemOfInterest) && _eggsInRadius.Count <= 0) return;
            
            _eggsInRadius.Remove(dogItemOfInterest);
        }
    }
}
