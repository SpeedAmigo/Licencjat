using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerOxygen : MonoBehaviour
{
    private Slider _oxygenSlider;

    private void Awake()
    {
        _oxygenSlider = GetComponent<Slider>();
        
        OxygenScript.OnMaxStaminaEvent += SetMaxOxygen;
        OxygenScript.OnCurrentStaminaEvent += SetCurrentOxygen;
    }

    private void SetMaxOxygen(float maxOxygen)
    {
        _oxygenSlider.maxValue = maxOxygen;
    }

    private void SetCurrentOxygen(float currentOxygen)
    {
        _oxygenSlider.value = currentOxygen;
    }

    private void OnEnable()
    {

    }

    private void OnDestroy()
    {
        OxygenScript.OnMaxStaminaEvent -= SetMaxOxygen;
        OxygenScript.OnCurrentStaminaEvent -= SetCurrentOxygen;
    }
}
