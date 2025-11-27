using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float sensitivity = 15f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private Transform cameraHolder;
    
    private InputSystem_Actions _inputSystem;
    private Camera _playerCamera;
    [SerializeField] private GameObject playerCameraPrefab;

    private float _pitch;
    private Vector2 _lookInput;

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
        }
        else
        {
            enabled = false;
        }
    }

    private void Awake()
    {
        _inputSystem = new InputSystem_Actions();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        _inputSystem.Enable();
        _inputSystem.Player.Look.performed += OnLook;
        _inputSystem.Player.Look.canceled += OnLookCancelled;
    }

    private void OnDisable()
    {
        _inputSystem.Disable();
        _inputSystem.Player.Look.performed -= OnLook;
        _inputSystem.Player.Look.canceled -= OnLookCancelled;
    }

    private void LateUpdate()
    {
        RotationHandler();
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
    }
}
