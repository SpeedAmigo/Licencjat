using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;

public class SpawnerManager : NetworkBehaviour
{
    public static SpawnerManager Instance;

    [Header("Spawners")]
    [SerializeField] private int spawnersToEnable;
    [SerializeField] private List<ObjectSpawnerScript> spawners;
    [SerializeField] private List<EggSpawner> eggSpawners;
    
    public int dayNumber;
    
    [Header("Spawned Objects")]
    public List<NetworkObject> spawnedObjects;
    public List<NetworkObject> spawnedEggs; 

    [Header("Quota info")]
    [SerializeField, Range(0,1)] private float increasePercentage; 
    [SerializeField] private uint currentQuota;
    public uint targetQuota;

    [Header("Spawned Objects complete value")]
    public uint currentlySpawnedValue;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        ObjectSpawnerScript.OnObjectSpawned += AddSpawnedObject;
        ObjectSpawnerScript.OnValueAdd += AddToCurrentlySpawnedValue;
        EggSpawner.EggSpawned += OnAddSpawnedEgg;
    }
    
    private void OnDisable()
    {
        ObjectSpawnerScript.OnObjectSpawned -= AddSpawnedObject;
        ObjectSpawnerScript.OnValueAdd -= AddToCurrentlySpawnedValue;
    }

    private void AddToCurrentlySpawnedValue(uint value)
    {
        currentlySpawnedValue += value;
    }
    
    private void OnAddSpawnedEgg(NetworkObject obj)
    {
        if (spawnedEggs.Contains(obj)) return;
        spawnedEggs.Add(obj);
    }

    private void AddSpawnedObject(NetworkObject obj)
    {
        if (spawnedObjects.Contains(obj)) return;
        spawnedObjects.Add(obj);
    }
    
    [ContextMenu("Spawn Animals")]
    [ServerRpc(RequireOwnership = false)]
    public void StartSpawning()
    {
        SetTargetQuota();
        
        var pickedSpawners = PickSpawners();
        
        float perSpawnerValue = targetQuota / pickedSpawners.Count;
        
        foreach (var spawner in pickedSpawners)
        {
            spawner.valueToSpawn = perSpawnerValue;
            spawner.SpawnObject();
        }

        foreach (var egg in eggSpawners)
        {
            egg.SpawnObject();
        }
    }
    
    [ContextMenu("Despawn Animals")]
    [ServerRpc(RequireOwnership = false)]
    public void RemoveSpawnedObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            NetworkObject nob = spawnedObjects[i];

            if (nob == null) continue;
            
            spawnedObjects.RemoveAt(i);
            
            if (!nob.IsSpawned) continue;
            
            Despawn(nob);
        }
                
        foreach (var spawner in spawners)
        {
            spawner.valueToSpawn = 0;
            spawner.spawnedValue = 0;
        }
        
        currentlySpawnedValue = 0;
    }

    [Button]
    [ServerRpc(RequireOwnership = false)]
    public void RemoveSpawnedEggs()
    {
        for (int i = spawnedEggs.Count - 1; i >= 0; i--)
        {
            NetworkObject nob = spawnedEggs[i];

            if (nob == null) continue;
            
            if (nob.transform.position.y >= 200f) continue;
            
            spawnedEggs.RemoveAt(i);
            
            if (!nob.IsSpawned) continue;
            
            Despawn(nob);
        }
        
        foreach (var spawner in eggSpawners)
        {
            spawner.spawnedValue = 0;
        }
    }

    private List<ObjectSpawnerScript> PickSpawners()
    {
        List<ObjectSpawnerScript> availableSpawners = new List<ObjectSpawnerScript>();
        
        if (spawnersToEnable <= 0)
        {
            Debug.LogWarning("SpawnerManager: Spawners can not be less than zero");
            return null;
        }

        if (spawnersToEnable > spawners.Count)
        {
            Debug.LogWarning("SpawnerManager: Not enough spawners available");
            spawnersToEnable = spawners.Count;
        }

        foreach (var spawner in spawners)
        {
            if (spawner.enableOnDay <= dayNumber)
            {
                availableSpawners.Add(spawner);
            }
        }

        if (availableSpawners.Count == 0)
        {
            Debug.LogWarning("SpawnerManager: No available spawners");
            return null;
        }
        
        List<ObjectSpawnerScript> shuffled = new List<ObjectSpawnerScript>(availableSpawners);
        
        for (int i = 0; i < shuffled.Count; i++)
        {
            int rand = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[rand]) = (shuffled[rand], shuffled[i]);
        }
        
        return shuffled.GetRange(0, spawnersToEnable);
    }

    private void SetTargetQuota()
    {
        currentQuota = QuotaManagerScript.Instance.targetQuota.Value;
        targetQuota = AddQuotaOffset(currentQuota);
    }

    private uint AddQuotaOffset(uint currentQuota)
    {
        return currentQuota + (uint)(currentQuota * increasePercentage);
    }
}
