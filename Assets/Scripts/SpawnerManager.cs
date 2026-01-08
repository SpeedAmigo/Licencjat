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
    
    [Header("Spawned Objects")]
    [AllowMutableSyncType] private SyncList<GameObject> spawnedObjects;

    [Header("Quota info")]
    [SerializeField, Range(0,1)] private float increasePercentage; 
    [SerializeField] private uint currentQuota;
    public uint targetQuota;

    [Header("Spawned Objects complete value")]
    [SerializeField] private uint currentlySpawnedValue;
    
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
    
    [ServerRpc(RequireOwnership = false)]
    public void StopSpawning()
    {
        // tell the spawners to stop spawning
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveSpawnedObjects()
    {
        // remove all spawned objects or animals from the map
    }

    private List<ObjectSpawnerScript> PickSpawners()
    {
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
        
        List<ObjectSpawnerScript> shuffled = new List<ObjectSpawnerScript>(spawners);
        
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
