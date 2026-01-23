using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerOxygen : NetworkBehaviour
{
    private Slider _oxygenSlider;

    private void Awake()
    {
        _oxygenSlider = GetComponent<Slider>();
    }

    private void SetMaxOxygen(float maxOxygen)
    {
        if (!IsOwner) return;
        _oxygenSlider.maxValue = maxOxygen;
    }

    private void SetCurrentOxygen(float currentOxygen)
    {
        if (!IsOwner) return;
        _oxygenSlider.value = currentOxygen;
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
