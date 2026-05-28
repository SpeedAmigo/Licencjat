using System;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;
using Sirenix.OdinInspector;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : PlayerComponent
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
    
    [Header("Spectate Settings")]
    [SerializeField] private float spectateSpeed = 10f;
    [SerializeField] private float spectateSprintSpeed = 20f;

    [SerializeField] private float scrollSpeedStep = 2f;
    [SerializeField] private float minSpectateSpeed = 2f;
    [SerializeField] private float maxSpectateSpeed = 50f;

    private float _currentSpectateSpeed;
    
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

    private bool _isSpectating;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            _inputSystem.Disable();
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        
        _inputSystem = new InputSystem_Actions();
        _controller = GetComponent<CharacterController>();
        
        _moveSpeed = walkingSpeed;
    }

    private void Start()
    {
        currentStamina = maxStamina;
        
        _currentSpectateSpeed = spectateSpeed;
        
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

    protected override void OnEnable()
    {
        base.OnEnable();
        
        _inputSystem.Enable();
        _inputSystem.Player.Move.performed += OnMove;
        _inputSystem.Player.Move.canceled += OnMoveCancelled;
        _inputSystem.Player.Jump.performed += OnJump;

        UIConsoleScript.OnConsoleOpen += HandleInput;
        playerRoot.StunEvent += OnStunHandle;
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

        horizontalCurrent = Vector3.Lerp(
            horizontalCurrent,
            move,
            decelerationSpeed * Time.deltaTime
        );

        _currentMove.x = horizontalCurrent.x;
        _currentMove.z = horizontalCurrent.z;
    }

    private void OnSprint()
    {
        bool sprintKey = _inputSystem.Player.Sprint.IsPressed();
        bool isMoving = _moveInput.sqrMagnitude > 0.01f;

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
        if (!IsOwner) return;
        
        if (_isSpectating)
        {
            SpectateMovement();
            return;
        }
        
        // normal movement
        isGrounded = _controller.isGrounded;
        
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

        /*if (_controller.enabled)
        {
            _controller.Move(_velocity * Time.deltaTime);
        }*/
        
        if (IsOwner)
        {
            if (!playerRoot.isAlive.Value) return;
            MoveHandler();
            OnSprint();
            
            Vector3 finalMove = _currentMove;
            finalMove.y = _velocity.y;

            _controller.Move(finalMove * Time.deltaTime);
            
            OnCurrentStamina?.Invoke(currentStamina);
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
    
    private void OnStunHandle(bool stunned, float duration)
    {
        if (!IsOwner) return;

        if (stunned)
        {
            _inputSystem.Disable();
        }
        else
        {
            _inputSystem.Enable();
        }
    }

    protected override void DeathHandle()
    {
        _controller.detectCollisions = false;
    }

    protected override void ReviveHandle()
    {
        _controller.detectCollisions = true;
    }

    protected override void SpectateHandle(bool value)
    {
        _isSpectating = value;
    }
    
    private void SpectateMovement()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        
        if (scroll != 0)
        {
            _currentSpectateSpeed += scroll * scrollSpeedStep * Time.deltaTime;
            
            _currentSpectateSpeed = Mathf.Clamp(
                _currentSpectateSpeed,
                minSpectateSpeed,
                maxSpectateSpeed
            );
        }
        
        Transform cam = Camera.main.transform;

        // Camera-relative movement
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        Vector3 move =
            forward * _moveInput.y +
            right * _moveInput.x;

        // Optional vertical movement
        if (Keyboard.current.spaceKey.isPressed)
        {
            move += Vector3.up;
        }

        if (Keyboard.current.leftCtrlKey.isPressed)
        {
            move += Vector3.down;
        }

        move.Normalize();
        
        float speed = _inputSystem.Player.Sprint.IsPressed()
            ? _currentSpectateSpeed * 2f
            : _currentSpectateSpeed;

        _controller.Move(move * speed * Time.deltaTime);
    }
}