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
    public void RegisterCameraHolder(Transform cameraHolder)
    {
        if (cameraHolders.Contains(cameraHolder)) return;
        
        cameraHolders.Add(cameraHolder);
    }
    
    /*[ServerRpc(RequireOwnership = false)]
    public void UnregisterCameraHolder(NetworkObject cameraHolder)
    {
        cameraHolders.Remove(cameraHolder);
    }*/

    public void SwitchUp()
    {
        if (cameraHolders.Count == 0) return;
        
        if (!_isSpectating)
        {
            _isSpectating = true;
            _currentIndex = 0;
        }
        else
        {
            _currentIndex = (_currentIndex + 1) % cameraHolders.Count;
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
        
        _playerCamera.transform.SetParent(cameraHolders[_currentIndex].transform);
        _playerCamera.transform.localPosition = Vector3.zero;
        _playerCamera.transform.localRotation = Quaternion.identity;
    }
}

