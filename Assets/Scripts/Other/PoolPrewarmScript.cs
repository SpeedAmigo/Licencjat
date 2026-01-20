using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;

public class PoolPrewarmScript : MonoBehaviour
{
    [SerializeField] private List<PrewarmObject> prewarmObjects = new();

    private void Start()
    {
        foreach (var obj in prewarmObjects)
        {
            if (obj.nob == null || obj.quantity == 0) continue;
            InstanceFinder.NetworkManager.CacheObjects(obj.nob, obj.quantity, obj.asServer);
        }
    }
}

[Serializable]
public class PrewarmObject
{
    public NetworkObject nob;
    public int quantity;
    public bool asServer;
}
