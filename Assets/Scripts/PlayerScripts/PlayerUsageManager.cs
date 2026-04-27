using System;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class PlayerUsageManager : NetworkBehaviour
{
    public static PlayerUsageManager Instance;

    public static event Action<float> StartUsage;
    public static event Action StopUsage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (Instance != this)
            {
                Destroy(this);
            }
        }
    }
    
    public void StartFillUsage(float usageTime)
    {
        StartUsage?.Invoke(usageTime);
    }
    
    public void StopFillUsage()
    {
        StopUsage?.Invoke();
    }
}
