using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuScript : NetworkBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private CameraController cameraController;

    private InputSystem_Actions _inputSystem;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
        }
    }

    private void Awake()
    {
        cameraController = GetComponentInParent<CameraController>();
        _inputSystem = new InputSystem_Actions();
        pauseMenu.SetActive(false);
    }
    
    private void OnEnable()
    {
        _inputSystem.Enable();
        _inputSystem.Player.Close.performed += OnEscape;
    }
    
    private void OnDisable()
    {
        _inputSystem.Disable();
        _inputSystem.Player.Close.performed -= OnEscape;
    }

    private void OnEscape(InputAction.CallbackContext obj)
    {
        bool value = !pauseMenu.activeInHierarchy;
        ShowMenu(value);
    }
    
    [Client]
    private void ShowMenu(bool value)
    {
        pauseMenu.SetActive(value);
        if (value)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cameraController.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cameraController.enabled = true;
        }
    }
    
    public void OnResumeButton()
    {
        ShowMenu(false);
    }
    
    public void OnLeaveButton()
    {
        if (IsOwner)
        {
            var networkManager = NetworkManager.ClientManager;
            if (networkManager != null)
            {
                networkManager.StopConnection();
            }
            
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }

    public void OnExitButton()
    {
        var networkManager = NetworkManager.ClientManager;
        if (networkManager != null)
        {
            networkManager.StopConnection();
        }
        
        Application.Quit();
    }
}
