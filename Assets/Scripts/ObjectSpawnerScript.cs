using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectSpawnerScript : NetworkBehaviour
{
    [Header("Spawner settings")]
    public int enableOnDay = 1;
    public bool canSpawn = false;
    
    [Header("Assigned value to spawn")]
    public float valueToSpawn;
    [SerializeField] private float spawnedValue;
    
    [Header("Spawnable objects list")]
    [SerializeField] private List<SpawnAbleObject> spawnAbleObjects;

    private void Update()
    {
        if (!IsServerInitialized) return;

        if (spawnedValue < valueToSpawn)
        {
            SpawnObject();
        }
    }
    
    private void SpawnObject()
    {
        var pickedObject = PickObjectToSpawn();
        var pickedValue = PickRandomValue(pickedObject.minMaxSellValue);
        
        var spawnedObject = Instantiate(pickedObject.prefab, transform.position, Quaternion.identity);
        Spawn(spawnedObject);

        spawnedObject.GetComponent<ObjectValue>().actualSellValue.Value = pickedValue;
        
        SpawnerManager.Instance.spawnedObjects.Add(spawnedObject);
        
        spawnedValue += pickedValue;
        SpawnerManager.Instance.currentlySpawnedValue += (uint)pickedValue;
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
    
    private int PickRandomValue(Vector2Int range)
    {
        return Random.Range(range.x, range.y + 1);
    }
}

[Serializable]
public class SpawnAbleObject
{
    public GameObject prefab;
    public float spawnChance;
    public int availableSinceDay;
    public Vector2Int minMaxBuyValue;
    public Vector2Int minMaxSellValue;
}
