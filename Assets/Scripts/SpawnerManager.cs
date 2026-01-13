using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class SpawnerManager : NetworkBehaviour
{
    public static SpawnerManager Instance;

    [Header("Spawners")]
    [SerializeField] private int spawnersToEnable;
    [SerializeField] private List<ObjectSpawnerScript> spawners;
    
    public int dayNumber;
    
    [Header("Spawned Objects")]
    public List<GameObject> spawnedObjects;

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
    
    // just for now
    private void Start()
    {
        Invoke(nameof(StartSpawning), 2f);
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
        }
    }
    
    [ContextMenu("Despawn Animals")]
    [ServerRpc(RequireOwnership = false)]
    public void RemoveSpawnedObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = spawnedObjects[i];

            if (obj == null) continue;
            
            spawnedObjects.RemoveAt(i);
            Destroy(obj);
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
