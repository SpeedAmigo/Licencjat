using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using Heathen.SteamworksIntegration;
using UnityEngine;
using UnityEngine.UI;
using InputAction = UnityEngine.InputSystem.InputAction;

public class PauseMenuScript : NetworkBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Button invitationButton;

    private InputSystem_Actions _inputSystem;
    
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
        SpaceShipConsoleScript.ActivateInvitationButton += EnableInvitationButton;
    }
    
    private void OnDisable()
    {
        _inputSystem.Disable();
        _inputSystem.Player.Close.performed -= OnEscape;
        SpaceShipConsoleScript.ActivateInvitationButton += EnableInvitationButton;
    }

    private void EnableInvitationButton(bool value)
    {
        invitationButton.interactable = value;
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
        if (!IsOwner) return;
        
        var networkManager = NetworkManager.ClientManager;
        if (networkManager == null) return;
        
        
        if (IsClientInitialized && !IsServerInitialized)
        {
            ConnectionManager.Instance?.StopConnection();
            networkManager.StopConnection();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
            return;
        }

        if (IsServerInitialized)
        {
            DisconnectAllClients(false);
            ConnectionManager.Instance?.StopConnection();
            networkManager.StopConnection();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }

    public void OnInviteFriendsButton()
    {
        Heathen.SteamworksIntegration.API.Overlay.Client.Activate(OverlayDialog.friends);
    }

    public void OnExitButton()
    {
        if (!IsOwner) return;
        
        var networkManager = NetworkManager.ClientManager;
        if (networkManager == null) return;
        
        if (IsServerInitialized)
        {
            DisconnectAllClients(true);
            ConnectionManager.Instance?.StopConnection();
            networkManager.StopConnection();
            Application.Quit();
        }
        
        if (IsClientInitialized && !IsServerInitialized)
        {
            ConnectionManager.Instance?.StopConnection();
            networkManager.StopConnection();
            Application.Quit();
        }
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
        }
    }
    
    private void DisconnectAllClients(bool exitGameForClient)
    {
        var networkManager = NetworkManager.ServerManager;
        if (networkManager == null) return;
        
        var clients = new List<NetworkConnection>(networkManager.Clients.Values);
        
        foreach (var client in clients)
        {
            client.Disconnect(true);
            if (exitGameForClient)
            {
                Application.Quit();
            }
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (!IsServerInitialized)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }
}
