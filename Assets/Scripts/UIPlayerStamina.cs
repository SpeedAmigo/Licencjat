using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStamina : MonoBehaviour
{
    private Slider staminaSlider;

    private void Awake()
    {
        staminaSlider = GetComponent<Slider>();
    }

    private void SetStaminaBar(float currentStamina)
    {
        staminaSlider.value = currentStamina;
    }

    private void SetMaxStaminaBar(float maxStamina)
    {
        staminaSlider.maxValue = maxStamina;
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
