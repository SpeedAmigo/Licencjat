using System;
using System.Linq;
using Commands;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class QuotaManagerScript : NetworkBehaviour
{
    public static QuotaManagerScript Instance;
    
    [AllowMutableSyncType] public SyncVar<uint> currentMoney;
    [AllowMutableSyncType] public SyncVar<uint> targetQuota;

    [SerializeField] private uint baseIncreaseValue; //static value
    [SerializeField] private float threshold = 0.2f; //20%
    [SerializeField] private float excessMultiplier = 0.2f; // 20%

    public static event Action<uint> OnMoneyChanged;
    public static event Action<uint> OnTargetQuotaChanged;

    private void Awake()
    {
        Instance = this;
        
        CommandsManager.Instance.RegisterInstance(this);
    }

    private void Start()
    {
        if (!IsServerInitialized) return;
        
        OnMoneyChanged?.Invoke(currentMoney.Value);
        OnTargetQuotaChanged?.Invoke(targetQuota.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddMoney(uint value)
    {
        currentMoney.Value += value;
        OnMoneyChanged?.Invoke(currentMoney.Value);
    }
    
    public void CompareQuota()
    {
        if (!IsServerInitialized) return;
        
        OnMoneyChanged?.Invoke(currentMoney.Value);
        
        if (currentMoney.Value >= targetQuota.Value)
        {
            Debug.Log("Quota reached!");
            IncreaseQuota();
            GameOverManager.Instance.ReviveDeadPlayersServer();
            GameOverManager.Instance.RestoreOxygen();
        }
        else
        {
            Debug.Log($"collected money: {currentMoney.Value}");
            Debug.Log($"target quota: {targetQuota.Value}");
            
            Debug.Log("You are fired!");
            GameOverManager.Instance.GameOverServer(true);
        }
    }

    private void IncreaseQuota()
    {
        uint excess = currentMoney.Value - targetQuota.Value;
        
        float excessPercentage = (float)excess / targetQuota.Value;
        
        uint additionalWage = (uint)(excess * excessMultiplier);

        uint increaseValue;

        if (excessPercentage > threshold)
        {
            increaseValue = baseIncreaseValue + additionalWage;
        }
        else
        {
            increaseValue = baseIncreaseValue;
        }
        
        targetQuota.Value += increaseValue;
        OnTargetQuotaChanged?.Invoke(targetQuota.Value);
    }

    [Command("SetCurrentMoney", "Sets current money")]
    public void SetCurrentMoney(uint value)
    {
        AddMoney(value);
    }
}
