using System;
using System.Linq;
using Commands;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIConsoleScript : MonoBehaviour
{
    public static event Action<bool> OnConsoleOpen;
    
    private InputSystem_Actions inputSystem;
    [SerializeField] private GameObject console;

    private void Awake()
    {
        inputSystem = new();
        
        CommandsManager.Instance.RegisterInstance(this);
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
            OnConsoleOpen?.Invoke(true);
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            OnConsoleOpen?.Invoke(false);
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
