using FishNet.Object;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;

public class ColorChanger : NetworkBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    
    private InputSystem_Actions _inputSystem;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
        }
    }
    
    private void Awake()
    {
        _inputSystem = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputSystem.Enable();
        _inputSystem.Player.Interact.performed += OnInteraction;
    }

    private void OnDisable()
    {
        _inputSystem.Disable();
        _inputSystem.Player.Interact.performed -= OnInteraction;
    }
    
    private void OnInteraction(InputAction.CallbackContext context)
    {
        if (!context.performed || !IsOwner) return;
        ChangeColor(Random.ColorHSV());
    }

    [ServerRpc]
    public void ChangeColor(Color color)
    {
        ChangeColor_Client(color);
    }

    [ObserversRpc]
    private void ChangeColor_Client(Color color)
    {
        if (targetRenderer != null)
        {
            targetRenderer.material.color = color;
        }
    }
}
