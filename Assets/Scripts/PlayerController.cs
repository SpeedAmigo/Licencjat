using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Connection;
using FishNet.Object;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkingSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float fallMultiplier = 2f;
    [SerializeField] private float jumpMultiplier = 2f;
    
    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -9.8f;
    
    private InputSystem_Actions _inputSystem;
    private CharacterController _controller;
    
    private Vector2 _moveInput;
    private bool _isSprinting;
    private float _moveSpeed;

    private Vector3 _velocity;
    private bool isGrounded;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            _inputSystem.Disable();
        }
    }
    
    private void Awake()
    {
        _inputSystem = new InputSystem_Actions();
        _controller = GetComponent<CharacterController>();
        
        _moveSpeed = walkingSpeed;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCancelled(InputAction.CallbackContext context)
    {
        _moveInput = Vector2.zero;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(jumpForce * -2f * gravity * jumpMultiplier);
            _velocity.y = jumpVelocity;
        }
    }

    private void OnEnable()
    {
        _inputSystem.Enable();
        _inputSystem.Player.Move.performed += OnMove;
        _inputSystem.Player.Move.canceled += OnMoveCancelled;
        _inputSystem.Player.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        _inputSystem.Disable();    
        _inputSystem.Player.Move.performed -= OnMove;
        _inputSystem.Player.Move.canceled -= OnMoveCancelled;
        _inputSystem.Player.Jump.performed -= OnJump;
    }
    
    private void MoveHandler()
    {
        Vector3 move = new Vector3(_moveInput.x, 0, _moveInput.y);
        move = transform.TransformDirection(move);
        
        _controller.Move(move * _moveSpeed * Time.deltaTime);
    }

    private void OnSprint()
    {
        _isSprinting = _inputSystem.Player.Sprint.IsPressed();
        _moveSpeed = _isSprinting ? sprintSpeed : walkingSpeed;
    }
    
    private void Update()
    {
        isGrounded = _controller.isGrounded;

        if (IsOwner)
        {
            MoveHandler();
            OnSprint();
        }
        
        if (isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        if (_velocity.y < 0)
        {
            _velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else
        {
            _velocity.y += gravity * jumpMultiplier * Time.deltaTime;
        }

        if (_controller.enabled)
        {
            _controller.Move(_velocity * Time.deltaTime);
        }
    }
}