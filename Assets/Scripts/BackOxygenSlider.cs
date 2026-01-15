using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class BackOxygenSlider : NetworkBehaviour
{
    private Slider _slider;
    private float _timer;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    /*public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
        {
            enabled = false;
        }
    }*/
    
    private void SetMaxOxygen(float maxOxygen)
    {
        SetMaxOxygenServer(maxOxygen);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMaxOxygenServer(float maxOxygen)
    {
        SetMaxOxygenClient(maxOxygen);
    }

    [ObserversRpc(BufferLast = true)]
    private void SetMaxOxygenClient(float maxOxygen)
    {
        _slider.maxValue = maxOxygen; 
    }
    
    private void SetCurrentOxygen(float currentOxygen)
    {
        _slider.value = currentOxygen;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCurrentOxygenServer(float currentOxygen)
    {
        SetCurrentOxygenClient(currentOxygen);
    }

    [ObserversRpc]
    private void SetCurrentOxygenClient(float currentOxygen)
    {
        _slider.value = currentOxygen;
    }
    
    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= 1f)
        {
            _timer = 0;
            SetCurrentOxygenServer(_slider.value);
        }
    }
    
    private void OnEnable()
    {
        OxygenScript.OnMaxStaminaEvent += SetMaxOxygen;
        OxygenScript.OnCurrentStaminaEvent += SetCurrentOxygen;
    }

    private void OnDisable()
    {
        OxygenScript.OnMaxStaminaEvent -= SetMaxOxygen;
        OxygenScript.OnCurrentStaminaEvent -= SetCurrentOxygen;
    }
}
