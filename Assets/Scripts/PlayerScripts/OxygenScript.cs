using System;
using Commands;
using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class OxygenScript : NetworkBehaviour
{
    #region Events
    public static event Action<float> OnMaxStaminaEvent;
    public static event Action<float> OnCurrentStaminaEvent;
    public static event Action<float> OnDrainRateEvent;
    public event Action OnDieEvent;
    
    #endregion

    #region SyncVars
    
    [AllowMutableSyncType] public SyncVar<bool> hasOxygen;
    [AllowMutableSyncType] public SyncVar<bool> canDrainOxygen;
    
    [AllowMutableSyncType] public SyncVar<float> maxOxygen;
    [AllowMutableSyncType] public SyncVar<float> currentOxygen;
    
    [AllowMutableSyncType] public SyncVar<float> baseDrainRate;
    [AllowMutableSyncType] public SyncVar<float> drainRate;
    
    #endregion
    
    #region Variables
    
    [SerializeField] private LayerMask stopOxygenDrainingLayers;
    
    
    private int _safeZoneCount = 0;
    private float _lastDrainRate = -1f;
    
    #endregion
    
    #region Commands

    [Command("SetCurrentOxygen", "Sets the current amount of oxygen.")]
    [ServerRpc(RequireOwnership = false)]
    public void SetOxygen(float value)
    {
        if (value > maxOxygen.Value)
        {
            currentOxygen.Value = maxOxygen.Value;
        }
        else if (value < 0)
        {
            currentOxygen.Value = 0;
        }
        
        currentOxygen.Value = value;
        UpdateCurrentStaminaTarget(Owner, currentOxygen.Value);
    }

    [Command("CanDrainOxygen", "Set if oxygen can be drained.")]
    [ServerRpc(RequireOwnership = false)]
    public void CanDrainOxygen(bool value)
    {
        canDrainOxygen.Value = value;
    }
    
    [Command("SetDrainRate", "Sets the current drain rate of oxygen.")]
    [ServerRpc(RequireOwnership = false)]
    public void SetDrainRate(float value)
    {
        drainRate.Value = value;
        UpdateDrainRate(Owner, drainRate.Value);
    }
    
    #endregion
    
    public override void OnStartServer()
    {
        currentOxygen.Value = maxOxygen.Value;
        hasOxygen.Value = true;
        
        TimeManager.OnTick += Tick;
    }

    public override void OnStopServer()
    {
        TimeManager.OnTick -= Tick;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        InitialOxygenValues();
    }

    private void Start()
    {
        CommandsManager.Instance.RegisterInstance(this);
    }
    
    private void Tick()
    {
        if (!hasOxygen.Value) return;
        
        if (!Mathf.Approximately(drainRate.Value, _lastDrainRate))
        {
            UpdateDrainRate(Owner, drainRate.Value);
            _lastDrainRate = drainRate.Value;
            
            //Debug.Log($"drainRate: {drainRate.Value}, LastDrainRate: {_lastDrainRate}");
        }
        
        if (!canDrainOxygen.Value) return;
        
        currentOxygen.Value -= drainRate.Value * (float)TimeManager.TickDelta;
        UpdateCurrentStaminaTarget(Owner, currentOxygen.Value);
        
        if (currentOxygen.Value <= 0f)
        {
            currentOxygen.Value = 0;
            hasOxygen.Value = false;
            TargetDie(Owner);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InitialOxygenValues()
    {
        UpdateMaxStaminaTarget(Owner, maxOxygen.Value);
        UpdateCurrentStaminaTarget(Owner, currentOxygen.Value);
    }

    [TargetRpc]
    public void UpdateCurrentStaminaTarget(NetworkConnection conn, float value)
    {
        OnCurrentStaminaEvent?.Invoke(value);
    }

    [TargetRpc]
    private void UpdateDrainRate(NetworkConnection conn, float value)
    {
        OnDrainRateEvent?.Invoke(value);
    }

    [TargetRpc]
    private void UpdateMaxStaminaTarget(NetworkConnection conn, float value)
    {
        OnMaxStaminaEvent?.Invoke(value);
    }

    [TargetRpc]
    private void TargetDie(NetworkConnection conn)
    {
        Debug.Log("You run out of oxygen!");
        OnDieEvent?.Invoke();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (IsLayerInMask(other.gameObject.layer, stopOxygenDrainingLayers))
        {
            _safeZoneCount++;
            SetCanDrainOxygen(false);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (IsLayerInMask(other.gameObject.layer, stopOxygenDrainingLayers))
        {
            _safeZoneCount--;
            SetCanDrainOxygen(_safeZoneCount <= 0);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetCanDrainOxygen(bool value)
    {
        canDrainOxygen.Value = value;
    }

    private bool IsLayerInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
