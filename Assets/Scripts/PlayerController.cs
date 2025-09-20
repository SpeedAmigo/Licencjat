using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Connection;
using FishNet.Object;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkingSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    
    [Header("Camera Settings")]
    [SerializeField] private float sensitivity = 15f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = -80f;
    [SerializeField] private Transform cameraHolder;
    
    private InputSystem_Actions _inputSystem;
    private InputAction _move;
    private InputAction _sprint;
    private InputAction _jump;
    private InputAction _look;
    
    private Rigidbody _rb;
    private Camera _playerCamera;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _isSprinting;

    private float _pitch;
    private float _moveSpeed;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            _playerCamera = Camera.main;
            _playerCamera.transform.SetParent(cameraHolder);
            _playerCamera.transform.localPosition = Vector3.zero;
        }
        else
        {
            _inputSystem.Disable();
        }
    }
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _moveSpeed = walkingSpeed;
        
        _inputSystem = new InputSystem_Actions();
        
        _move = _inputSystem.Player.Move;
        _move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _move.canceled += ctx => _moveInput = Vector2.zero;
        
        _sprint = _inputSystem.Player.Sprint;

        _jump = _inputSystem.Player.Jump;
        _jump.performed += ctx => JumpHandler();
        
        _look = _inputSystem.Player.Look;
        _look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        _look.canceled += ctx => _lookInput = Vector2.zero;
    }

    private void OnEnable()
    {
        _inputSystem.Enable();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        _inputSystem.Disable();    
    }
    
    private void MoveHandler()
    {
        Vector3 movement = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized * _moveSpeed;
        
        Vector3 velocity = transform.TransformDirection(movement);
        velocity.y = _rb.linearVelocity.y;

        _rb.linearVelocity = velocity;
    }

    private void OnSprint()
    {
        _isSprinting = _sprint.IsPressed();
        _moveSpeed = _isSprinting ? sprintSpeed : walkingSpeed;
    }

    private void JumpHandler()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, 1.5f)) return;

        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    
    private void RotationHandler()
    {
        transform.Rotate(Vector3.up * _lookInput.x * sensitivity * Time.deltaTime);
        
        _pitch -= _lookInput.y * sensitivity * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        
        cameraHolder.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }
    
    private void Update()
    {
        Debug.DrawRay(transform.position, Vector3.down * 1.5f, Color.red);
        
        RotationHandler();
        OnSprint();
    }
    
    private void FixedUpdate()
    {
        MoveHandler();
    }
}