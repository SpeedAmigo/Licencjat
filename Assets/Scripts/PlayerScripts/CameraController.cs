using System;
using System.Timers;
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
    
    [Header("Head Bob settings")]
    [SerializeField] private bool headBobEnabled = true;
    [SerializeField] private float walkToggleSpeed = 5f;
    [SerializeField] private float sprintToggleSpeed = 8f;

    [Header("Walk Headbob settings")]
    [SerializeField] private float walkAmplitude = 0.015f;
    [SerializeField] private float walkFrequency = 10f;
    
    [Header("Sprint Headbob settings")]
    [SerializeField] private float sprintAmplitude = 0.015f;
    [SerializeField] private float sprintFrequency = 10f;

    private float _currentAmplitude;
    private float _currentFrequency;

    private float _targetAmplitude;
    private float _targetFrequency;
    
    private float _bobTimer;
    
    private Vector3 _startPosition;
    private CharacterController _characterController;
    
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
            
            _startPosition = _playerCamera.transform.localPosition;
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
        
        _characterController = GetComponent<CharacterController>();
        
        SettingsScript.headBobSetting += HandleHeadBob;
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        _inputSystem.Enable();
        _inputSystem.Player.Look.performed += OnLook;
        _inputSystem.Player.Look.canceled += OnLookCancelled;
        _inputSystem.Player.Move.performed += HandleSwitch;

        UIConsoleScript.OnConsoleOpen += HandleInput;

        if (playerRoot == null) return;
        playerRoot.StunEvent += OnStunHandle;
    }
    
    private void CheckMotion()
    {
        float speed = new Vector3(_characterController.velocity.x, 0, _characterController.velocity.z).magnitude;
        
        if (speed < walkToggleSpeed || !_characterController.isGrounded)
        {
            ResetPosition(6f);
            return;
        }
        
        bool isSprinting = speed >= sprintToggleSpeed;

        _targetAmplitude = isSprinting ? sprintAmplitude : walkAmplitude;
        _targetFrequency = isSprinting ? sprintFrequency : walkFrequency;
        
        _currentAmplitude = Mathf.Lerp(_currentAmplitude, _targetAmplitude , Time.deltaTime * 6f);
        _currentFrequency = Mathf.Lerp(_currentFrequency, _targetFrequency , Time.deltaTime * 6f);
        
        Vector3 motion = FootStepMotion();
        _playerCamera.transform.localPosition = _startPosition + motion;
    }
    
    private Vector3 FootStepMotion()
    {
        _bobTimer += Time.deltaTime * _currentFrequency;
        
        Vector3 pos = Vector3.zero;
        
        pos.y += Mathf.Sin(_bobTimer) * _currentAmplitude;
        pos.x += Mathf.Sin(_bobTimer * 0.5f) * _currentAmplitude * 2;
        
        return pos;
    }

    private void ResetPosition(float time)
    {
        _playerCamera.transform.localPosition = Vector3.Lerp(_playerCamera.transform.localPosition, _startPosition, Time.deltaTime * time);
    }
    
    private void HandleHeadBob(bool value)
    {
        headBobEnabled = value;
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

        if (playerRoot == null) return;
        playerRoot.StunEvent -= OnStunHandle;
    }
    
    private void OnDestroy()
    {
        if (!IsOwner) return;
        SettingsScript.headBobSetting -= HandleHeadBob;
        
        /*if (CameraHoldersManager.Instance != null)
        {
            CameraHoldersManager.Instance.UnregisterCameraHolder(new CameraStruct(cameraHolder, gameObject.GetComponent<PlayerVisualController>(), OwnerId));
        }
        else
        {
            Debug.LogWarning("There is no camera holder manager");
        }*/
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        
        CameraHoldersManager.Instance.UnregisterCameraHolder(new CameraStruct(cameraHolder, gameObject.GetComponent<PlayerVisualController>(), OwnerId));
    }
    
    private void LateUpdate()
    {
        if (playerRoot.isAlive.Value)
        {
            RotationHandler();
            if (headBobEnabled)
            {
                CheckMotion();
            }
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
        armatureHolder.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
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

    [SerializeField] private GameObject stunHolder;
    private bool _cameraStunned;

    private void OnStunHandle(bool stunned, float duration)
    {
        if (!IsOwner) return;
        
        if (stunned)
        {
            headBobEnabled = false;
            ResetPosition(0f);
            
            _inputSystem.Disable();
            _playerCamera.transform.SetParent(stunHolder.transform);
        }
        else
        {
            if (SettingsScript.Instance.headBobEnabled)
            {
                headBobEnabled = true;    
            }
            
            _inputSystem.Enable();
            _playerCamera.transform.SetParent(cameraHolder);
            _playerCamera.transform.localPosition = Vector3.zero;
            _playerCamera.transform.localRotation = Quaternion.identity;
            
        }
    }

    [Preserve]
    protected override void ReviveHandle()
    {
        Debug.Log("ReviveHandle");
        CameraHoldersManager.Instance.AttachCameraToOriginalHolder(OwnerId);
    }
}
