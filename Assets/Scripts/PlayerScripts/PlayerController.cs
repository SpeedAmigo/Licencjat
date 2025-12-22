using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Connection;
using FishNet.Object;
using Sirenix.OdinInspector;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [GUIColor("Yellow")]
    [SerializeField] private float walkingSpeed = 5f;
    [GUIColor("Yellow")]
    [SerializeField] private float sprintSpeed = 8f;
    [GUIColor("Yellow")]
    [SerializeField] private float jumpForce = 5f;
    [GUIColor("Yellow")]
    [SerializeField] private float fallMultiplier = 2f;
    [GUIColor("Yellow")]
    [SerializeField] private float jumpMultiplier = 2f;
    
    [Header("Stamina Settings")]
    [GUIColor("Blue")]
    [SerializeField] private float currentStamina;
    [GUIColor("Yellow")]
    [SerializeField] private float maxStamina;
    [GUIColor("Yellow")]
    [SerializeField] private float staminaDrainRate;
    [GUIColor("Yellow")]
    [SerializeField] private float staminaRegenRate;
    
    [Header("Animator Settings")]
    [GUIColor("Yellow")]
    [SerializeField] private float idleAnimValue = 0f;
    [GUIColor("Yellow")]
    [SerializeField] private float walkAnimValue = 0.5f;
    [GUIColor("Yellow")]
    [SerializeField] private float sprintAnimValue = 1f;
    [GUIColor("Yellow")]
    [SerializeField] private float animatorLerpSpeed = 8f;

    public static event Action<float> OnMaxStamina;
    public static event Action<float> OnCurrentStamina;
    
    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -9.8f;
    
    private InputSystem_Actions _inputSystem;
    private CharacterController _controller;
    
    private Vector2 _moveInput;
    private bool _isSprinting;
    private float _moveSpeed;

    private Vector3 _velocity;
    private bool isGrounded;

    public float animatorVelocity;
    
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

    private void Start()
    {
        currentStamina = maxStamina;
        
        OnMaxStamina?.Invoke(maxStamina);
        OnCurrentStamina?.Invoke(currentStamina);
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
    
    private Vector3 _currentMove;
    [SerializeField] private float decelerationSpeed = 8f;
    
    private void MoveHandler()
    {
        Vector3 move = new Vector3(_moveInput.x, 0, _moveInput.y);
        move = transform.TransformDirection(move) * _moveSpeed;
        
        Vector3 horizontalCurrent = new Vector3(_currentMove.x, 0, _currentMove.z);
        horizontalCurrent = Vector3.Lerp(horizontalCurrent, move, decelerationSpeed * Time.deltaTime);
        
        _currentMove = new Vector3(horizontalCurrent.x, _velocity.y, horizontalCurrent.z);
        
        //_controller.Move(move * _moveSpeed * Time.deltaTime);
        _controller.Move(_currentMove * Time.deltaTime);
    }

    private void OnSprint()
    {
        bool sprintKey = _inputSystem.Player.Sprint.IsPressed();
        bool isMoving = _moveInput.sqrMagnitude > 0.01f;
        //_isSprinting = _inputSystem.Player.Sprint.IsPressed();

        if (sprintKey && isMoving && currentStamina > 0) // if key is pressed
        {
            _isSprinting = true;
            _moveSpeed = sprintSpeed;
            
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                _isSprinting = false;
                _moveSpeed = walkingSpeed;
            }
        }
        else // if key isn't pressed
        {
            _isSprinting = false;
            _moveSpeed = walkingSpeed;

            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                if (currentStamina > maxStamina)
                {
                    currentStamina = maxStamina;
                }
            }
        }
    }
    
    private void Update()
    {
        isGrounded = _controller.isGrounded;
        
        if (IsOwner)
        {
            MoveHandler();
            OnSprint();
            OnCurrentStamina?.Invoke(currentStamina);
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
        
        float targetAnim;
        bool isMoving = _moveInput.sqrMagnitude > 0.01f;
        
        if (!isMoving) targetAnim = idleAnimValue;
        else if (_isSprinting) targetAnim = sprintAnimValue;
        else targetAnim = walkAnimValue;

        animatorVelocity = Mathf.Lerp(animatorVelocity, targetAnim, animatorLerpSpeed * Time.deltaTime);
        
        if (Mathf.Abs(animatorVelocity) < 0.01f)
            animatorVelocity = 0f;
    }
}