using System;
using Commands;
using FishNet.CodeGenerating;
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
    
    [AllowMutableSyncType] public SyncVar<bool> canDrainOxygen;
    
    [AllowMutableSyncType] public SyncVar<float> maxOxygen;
    [AllowMutableSyncType] public SyncVar<float> currentOxygen;
    
    [AllowMutableSyncType] public SyncVar<float> baseDrainRate;
    [AllowMutableSyncType] public SyncVar<float> drainRate;
    
    #endregion
    
    #region Variables
    
    [SerializeField] private LayerMask stopOxygenDrainingLayers;
    
    private bool _hasOxygen;
    private int _safeZoneCount = 0;
    private float _lastDrainRate = -1f;
    
    #endregion
    
    #region Getters/Setters
    
    public float MaxOxygen => maxOxygen.Value;

    public float CurrentOxygen
    {
        get => currentOxygen.Value;
        set
        {
            currentOxygen.Value = value;
            OnCurrentStaminaEvent?.Invoke(currentOxygen.Value);
        }
    }

    public float DrainRate
    {
        get => drainRate.Value;
        set
        {
            drainRate.Value = value;
            OnDrainRateEvent?.Invoke(drainRate.Value);
        }
    }
    public float BaseDrainRate => baseDrainRate.Value;
    
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
        OnCurrentStaminaEvent?.Invoke(currentOxygen.Value);
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
    }

    #endregion
    
    public override void OnStartServer()
    {
        currentOxygen.Value = maxOxygen.Value;
        _hasOxygen = true;
        
        TimeManager.OnTick += Tick;
    }

    public override void OnStopServer()
    {
        TimeManager.OnTick -= Tick;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        //OnMaxStaminaEvent?.Invoke(maxOxygen.Value);
        //OnCurrentStaminaEvent?.Invoke(currentOxygen.Value);
        //CommandsManager.Instance.RegisterInstance(this);
    }

    private void Tick()
    {
        //if (!IsOwner) return;
        if (!_hasOxygen) return;
        if (!canDrainOxygen.Value) return;
        
        currentOxygen.Value -= drainRate.Value * (float)TimeManager.TickDelta;
        //OnCurrentStaminaEvent?.Invoke(currentOxygen.Value);

        if (currentOxygen.Value <= 0f)
        {
            currentOxygen.Value = 0;
            _hasOxygen = false;
            Debug.Log("You run out of oxygen!");
            OnDieEvent?.Invoke();
        }
        
        if (!Mathf.Approximately(drainRate.Value, _lastDrainRate))
        {
            OnDrainRateEvent?.Invoke(drainRate.Value);
            _lastDrainRate = drainRate.Value;
        }
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
