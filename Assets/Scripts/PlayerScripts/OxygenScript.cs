using System;
using Commands;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class OxygenScript : NetworkBehaviour
{
    public static event Action<float> OnMaxStaminaEvent;
    public static event Action<float> OnCurrentStaminaEvent;
    public event Action OnDieEvent;
    //public event Action OnReviveEvent;

    [SerializeField] private LayerMask stopOxygenDrainingLayers;
    [SerializeField] private DamageTemplate[] damageTemplates;

    public bool canDrainOxygen = true;
    
    [SerializeField] private float maxOxygen;
    [SerializeField] private float currentOxygen;
    
    [SerializeField] private float baseDrainRate;
    [SerializeField] private float drainRate;

    public float MaxOxygen => maxOxygen;

    public float CurrentOxygen
    {
        get => currentOxygen;
        set => currentOxygen = value;
    }

    public float DrainRate
    {
        get => drainRate;
        set => drainRate = value;
    }
    public float BaseDrainRate => baseDrainRate;

    private bool _hasOxygen;
    private int _safeZoneCount = 0;
    private float _lastDrainRate = -1f;
    
    private void Awake()
    {
        currentOxygen = maxOxygen;
        _hasOxygen = true;
    }

    private void Start()
    {
        OnMaxStaminaEvent?.Invoke(maxOxygen);
        OnCurrentStaminaEvent?.Invoke(currentOxygen);
        
        CommandsManager.Instance.RegisterInstance(this);
    }
    
    private void Update()
    {
        if (!IsOwner) return;
        
        if (!_hasOxygen) return;
        
        if (currentOxygen > 0 && canDrainOxygen)
        {
            currentOxygen -= drainRate * Time.deltaTime;
            OnCurrentStaminaEvent?.Invoke(currentOxygen);
        }
        else if (currentOxygen <= 0)
        {
            _hasOxygen = false;
            currentOxygen = 0;
            Debug.Log("You run out of oxygen!");
            OnDieEvent?.Invoke();
        }

        if (!Mathf.Approximately(drainRate, _lastDrainRate))
        {
            UpdateCracks();
            _lastDrainRate = drainRate;
        }
    }

    private void UpdateCracks()
    {
        foreach (var template in damageTemplates)
        {
            bool active = drainRate >= template.drainRate;
            foreach (var crack in template.cracksToActivate)
            {
                crack.SetActive(active);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsLayerInMask(other.gameObject.layer, stopOxygenDrainingLayers))
        {
            _safeZoneCount++;
            canDrainOxygen = false;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (IsLayerInMask(other.gameObject.layer, stopOxygenDrainingLayers))
        {
            _safeZoneCount--;
            canDrainOxygen = _safeZoneCount <= 0;
        }
    }

    private bool IsLayerInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    [Command("SetCurrentOxygen", "Sets the current amount of oxygen.")]
    public void SetOxygen(float value)
    {
        if (value > maxOxygen)
        {
            currentOxygen = maxOxygen;
        }
        else if (value < 0)
        {
            currentOxygen = 0;
        }
        
        currentOxygen = value;
        OnCurrentStaminaEvent?.Invoke(currentOxygen);
    }

    [Command("CanDrainOxygen", "Set if oxygen can be drained.")]
    public void CanDrainOxygen(bool value)
    {
        canDrainOxygen = value;
    }

    [Command("SetDrainRate", "Sets the current drain rate of oxygen.")]
    public void SetDrainRate(float value)
    {
        drainRate = value;
    }
}

[Serializable]
public class DamageTemplate
{
    public float drainRate;
    public GameObject[] cracksToActivate;
}
