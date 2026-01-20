using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraHoldersManager : NetworkBehaviour
{ 
    public static CameraHoldersManager Instance;

    [AllowMutableSyncType] private SyncList<Transform> cameraHolders = new();
    [AllowMutableSyncType] private SyncList<PlayerVisualController> playerVisualControllers = new();
    [AllowMutableSyncType] private SyncList<CameraStruct> cameraStructs = new();
    
    private bool _isSpectating = false;
    private int _currentIndex = 0;
    
    private Camera _playerCamera;
    
    private InputSystem_Actions _inputSystem;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        
        _inputSystem = new InputSystem_Actions();
        _playerCamera = Camera.main;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RegisterCameraHolder(CameraStruct cameraStruct)
    {
        if (cameraStructs.Contains(cameraStruct)) return;
        
        cameraStructs.Add(cameraStruct);
        
        
        /*if (cameraHolders.Contains(cameraHolder)) return;
        
        cameraHolders.Add(cameraHolder);
        
        PlayerVisualController visualController = playerGameObject.GetComponent<PlayerVisualController>();
        
        playerVisualControllers.Add(visualController);*/
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void UnregisterCameraHolder(CameraStruct cameraStruct)
    {
        if (!cameraStructs.Contains(cameraStruct)) return;
        
        cameraStructs.Remove(cameraStruct);
    }

    public void SwitchUp()
    {
        if (cameraStructs.Count == 0) return;
        
        if (!_isSpectating)
        {
            _isSpectating = true;
            _currentIndex = 0;
        }
        else
        {
            _currentIndex = (_currentIndex + 1) % cameraStructs.Count;
        }
        
        AttachCamera();
    }
    
    public void SwitchDown()
    {
        if (cameraHolders.Count == 0) return;
        
        if (!_isSpectating)
        {
            _isSpectating = true;
            _currentIndex = cameraHolders.Count - 1;
        }
        else
        {
            _currentIndex--;
            if (_currentIndex < 0)
                _currentIndex = cameraHolders.Count - 1;
        }
    }

    private void AttachCamera()
    {
        if (_playerCamera == null) return;
        
        _playerCamera.transform.SetParent(cameraStructs[_currentIndex].CameraHolder);
        _playerCamera.transform.localPosition = Vector3.zero;
        _playerCamera.transform.localRotation = Quaternion.identity;

        for (int i = 0; i < cameraStructs.Count; i++)
        {
            if (i == _currentIndex)
            {
                cameraStructs[i].VisualController.ChangeLayerOfVisual("ThisPlayer");
            }
            else
            {
                cameraStructs[i].VisualController.ChangeLayerOfVisual("Player");
            }
        }
    }
}

public struct CameraStruct
{
    public CameraStruct(Transform cameraHolder, PlayerVisualController visualController)
    {
        CameraHolder =  cameraHolder;
        VisualController = visualController;
    }
    
    public Transform CameraHolder;
    public PlayerVisualController VisualController;
}

