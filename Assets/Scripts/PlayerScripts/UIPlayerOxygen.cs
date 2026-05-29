using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerOxygen : MonoBehaviour
{
    private Slider _oxygenSlider;
    [SerializeField] private Animator alertAnimator;

    private void Awake()
    {
        _oxygenSlider = GetComponent<Slider>();
        
        OxygenScript.OnMaxStaminaEvent += SetMaxOxygen;
        OxygenScript.OnCurrentStaminaEvent += SetCurrentOxygen;
        OxygenScript.OxygenAlertEvent += HandleAlertAnimation;
    }

    private void HandleAlertAnimation(bool value)
    {
        alertAnimator.Play("OxygenAlertAnimation", 0, 0f);
        alertAnimator.enabled = value;

        alertAnimator.speed = value ? 1f : 0f;
        Debug.Log(value);
    }

    private void SetMaxOxygen(float maxOxygen)
    {
        _oxygenSlider.maxValue = maxOxygen;
    }

    private void SetCurrentOxygen(float currentOxygen)
    {
        _oxygenSlider.value = currentOxygen;
    }
    
    private void OnDestroy()
    {
        OxygenScript.OnMaxStaminaEvent -= SetMaxOxygen;
        OxygenScript.OnCurrentStaminaEvent -= SetCurrentOxygen;
    }
}
