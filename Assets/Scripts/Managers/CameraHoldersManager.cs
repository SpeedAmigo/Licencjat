using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraHoldersManager : NetworkBehaviour
{ 
    public static CameraHoldersManager Instance;
    
    [AllowMutableSyncType] private SyncList<CameraStruct> cameraStructs = new();
    
    private bool _isSpectating = false;
    private int _currentIndex = 0;
    private int _originalHolderIndex = 0;
    
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
    }
    
    [Server]
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
        if (cameraStructs.Count == 0) return;
        
        if (!_isSpectating)
        {
            _isSpectating = true;
            _currentIndex = cameraStructs.Count - 1;
        }
        else
        {
            _currentIndex--;
            if (_currentIndex < 0)
                _currentIndex = cameraStructs.Count - 1;
        }
        
        AttachCamera();
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
                cameraStructs[i].VisualController.ChangeVisualRender(false);
            }
            else
            {
                cameraStructs[i].VisualController.ChangeVisualRender(true);
            }
        }
    }

    public void AttachCameraToOriginalHolder(int ownerId)
    {
        if (_playerCamera == null) return;
        
        _originalHolderIndex = GetIndexByOwnerId(ownerId);
        
        if (_originalHolderIndex == -1)
        {
            Debug.LogWarning($"Camera holder not found for ownerId {ownerId}");
            return;
        }
        
        _isSpectating = false;
        _currentIndex = _originalHolderIndex;
        
        _playerCamera.transform.SetParent(cameraStructs[_originalHolderIndex].CameraHolder);
        _playerCamera.transform.localPosition = Vector3.zero;
        _playerCamera.transform.localRotation = Quaternion.identity;

        for (int i = 0; i < cameraStructs.Count; i++)
        {
            if (i == _originalHolderIndex)
            {
                cameraStructs[i].VisualController.ChangeVisualRender(false);
            }
            else
            {
                cameraStructs[i].VisualController.ChangeVisualRender(true);
            }
        }
    }

    private int GetIndexByOwnerId(int ownerId)
    {
        for (int i = 0; i < cameraStructs.Count; i++)
        {
            if (cameraStructs[i].ownerId == ownerId)
                return i;
        }

        return -1;
    }
}

public struct CameraStruct
{
    public CameraStruct(Transform cameraHolder, PlayerVisualController visualController, int id)
    {
        CameraHolder =  cameraHolder;
        VisualController = visualController;
        ownerId = id;
    }

    public int ownerId;
    public Transform CameraHolder;
    public PlayerVisualController VisualController;
}

