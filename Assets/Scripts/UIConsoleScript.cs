using UnityEngine;
using UnityEngine.InputSystem;

public class UIConsoleScript : MonoBehaviour
{
    private InputSystem_Actions inputSystem;
    [SerializeField] private GameObject console;

    private void Awake()
    {
        inputSystem = new();
    }

    private void OnEnable()
    {
        inputSystem.Enable();

        inputSystem.UI.ConsoleTrigger.performed += ConsoleTrigger;
    }
    
    private void OnDisable()
    {
        inputSystem.Disable();
        
        inputSystem.UI.ConsoleTrigger.performed -= ConsoleTrigger;
    }
    
    private void ConsoleTrigger(InputAction.CallbackContext obj)
    {
        bool isOpen = !console.activeSelf;
        console.SetActive(isOpen);

        if (isOpen)
        {
            inputSystem.Player.Disable();
            inputSystem.UI.Enable();
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            Debug.Log($"Console open: {isOpen}, Player enabled: {inputSystem.Player.enabled}");

        }
        else
        {
            inputSystem.Player.Enable();
            inputSystem.UI.Enable();
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
