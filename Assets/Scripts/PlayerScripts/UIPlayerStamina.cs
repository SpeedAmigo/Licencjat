using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStamina : MonoBehaviour
{
    private Slider _staminaSlider;

    private void Awake()
    {
        _staminaSlider = GetComponent<Slider>();
    }

    private void SetStaminaBar(float currentStamina)
    {
        _staminaSlider.value = currentStamina;
    }

    private void SetMaxStaminaBar(float maxStamina)
    {
        _staminaSlider.maxValue = maxStamina;
    }
    
    private void OnEnable()
    {
        PlayerController.OnCurrentStamina += SetStaminaBar;
        PlayerController.OnMaxStamina += SetMaxStaminaBar;
    }

    private void OnDisable()
    {
        PlayerController.OnCurrentStamina -= SetStaminaBar;
        PlayerController.OnMaxStamina -= SetMaxStaminaBar;
    }
}
