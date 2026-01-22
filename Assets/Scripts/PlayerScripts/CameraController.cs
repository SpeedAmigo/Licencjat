using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

public class CameraController : PlayerComponent
{
    [Header("Camera Settings")]
    [GUIColor("Yellow")]
    [SerializeField] private float sensitivity = 15f;
    [GUIColor("Yellow")]
    [SerializeField] private float minPitch = -80f;
    [GUIColor("Yellow")]
    [SerializeField] private float maxPitch = 80f;
    [GUIColor("Red")]
    [SerializeField] private Transform cameraHolder;
    [GUIColor("Red")]
    [SerializeField] private Transform armatureHolder;
    [GUIColor("Red")]
    [SerializeField] private GameObject playerCameraPrefab;
    
    private InputSystem_Actions _inputSystem;
    private Camera _playerCamera;
    
    private float _pitch;
    private Vector2 _lookInput;

    private Transform _originalCameraHolder;
    
    //private PlayerRoot _playerRoot;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            _playerCamera = Camera.main;
            if (_playerCamera != null)
            {
                _playerCamera.transform.SetParent(cameraHolder);
                _playerCamera.transform.localPosition = Vector3.zero;
                _playerCamera.transform.localRotation = Quaternion.identity;
            }
            
            _originalCameraHolder = cameraHolder;
        }
        else
        {
            enabled = false;
        }
        
        Invoke(nameof(RegisterCameraHolder), 2f);
    }

    private void RegisterCameraHolder()
    {
        if (CameraHoldersManager.Instance != null)
        {
            CameraHoldersManager.Instance.RegisterCameraHolder(new CameraStruct(cameraHolder, gameObject.GetComponent<PlayerVisualController>(), OwnerId));
        }
        else
        {
            Debug.LogWarning("There is no camera holder manager");
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        
        _inputSystem = new InputSystem_Actions();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        //_playerRoot = GetComponent<PlayerRoot>();
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        _inputSystem.Enable();
        _inputSystem.Player.Look.performed += OnLook;
        _inputSystem.Player.Look.canceled += OnLookCancelled;
        _inputSystem.Player.Move.performed += HandleSwitch;

        UIConsoleScript.OnConsoleOpen += HandleInput;
    }

    private void HandleInput(bool obj)
    {
        if (!obj)
        {
            _inputSystem.Enable();
        }
        else
        {
            _inputSystem.Disable();            
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        _inputSystem.Disable();
        _inputSystem.Player.Look.performed -= OnLook;
        _inputSystem.Player.Look.canceled -= OnLookCancelled;
        _inputSystem.Player.Move.performed -= HandleSwitch;
    }
    
    private void OnDestroy()
    {
        if (CameraHoldersManager.Instance != null)
        {
            CameraHoldersManager.Instance.UnregisterCameraHolder(new CameraStruct(cameraHolder, gameObject.GetComponent<PlayerVisualController>(), OwnerId));
        }
        else
        {
            Debug.LogWarning("There is no camera holder manager");
        }
    }
    
    private void LateUpdate()
    {
        if (playerRoot.isAlive.Value)
        {
            RotationHandler();
        }
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCancelled(InputAction.CallbackContext context)
    {
        _lookInput = Vector2.zero;
    }
    
    private void RotationHandler()
    {
        transform.Rotate(Vector3.up * _lookInput.x * (sensitivity * 0.1f));
        
        _pitch -= _lookInput.y * (sensitivity * 0.1f) ;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        
        cameraHolder.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        armatureHolder.localRotation = Quaternion.Euler(-_pitch, 0f, 0f);
    }
    
    private void HandleSwitch(InputAction.CallbackContext context)
    {
        if (playerRoot.isAlive.Value) return;
        
        Vector2 input = context.ReadValue<Vector2>();
        
        if (input.x > 0)
        { 
            CameraHoldersManager.Instance.SwitchUp();
        }
        else if (input.x < 0)
        {
            CameraHoldersManager.Instance.SwitchDown();
        }
    }

    [Preserve]
    protected override void ReviveHandle()
    {
        Debug.Log("ReviveHandle");
        CameraHoldersManager.Instance.AttachCameraToOriginalHolder(OwnerId);
    }
}
