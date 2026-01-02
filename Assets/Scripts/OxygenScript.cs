using System;
using FishNet.Object;
using UnityEngine;

public class OxygenScript : NetworkBehaviour
{
    public static event Action<float> OnMaxStaminaEvent;
    public static event Action<float> OnCurrentStaminaEvent;

    public static event Action OnDieEvent;
    
    [SerializeField] private float maxOxygen;
    [SerializeField] private float currentOxygen;

    [SerializeField] private float drainRate;

    private bool _hasOxygen;
    
    private void Awake()
    {
        currentOxygen = maxOxygen;
        _hasOxygen = true;
    }

    private void Start()
    {
        OnMaxStaminaEvent?.Invoke(maxOxygen);
        OnCurrentStaminaEvent?.Invoke(currentOxygen);
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        if (!_hasOxygen) return;

        if (currentOxygen > 0)
        {
            currentOxygen -= drainRate * Time.deltaTime;
            OnCurrentStaminaEvent?.Invoke(currentOxygen);
        }
        else if (currentOxygen < 0)
        {
            _hasOxygen = false;
            currentOxygen = 0;
            Debug.Log("You run out of oxygen!");
            OnDieEvent?.Invoke();
        }
    }
}
