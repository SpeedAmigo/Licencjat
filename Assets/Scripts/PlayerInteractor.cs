using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : NetworkBehaviour
{
    [SerializeField] private NetworkObject itemHolder;
    [SerializeField] private float interactionDistance;
    
    private InputSystem_Actions _inputSystem;
    private Camera _camera;

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
        _camera = Camera.main;
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
        if (!IsOwner) return;
        if (!context.performed) return;

        RaycastHit hit;

        if (!Physics.Raycast(_camera.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, interactionDistance)) return;

        if (hit.collider.TryGetComponent<NetworkObject>(out var netObj))
        {
            if (netObj.TryGetComponent<ObjectPickup>(out var pickup))
            {
                Pickup_Server(netObj, itemHolder);
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void Pickup_Server(NetworkObject obj, NetworkObject holder)
    {
        if (obj != null && obj.TryGetComponent<ObjectPickup>(out var pickup))
        {
            pickup.Pickup(holder);
        }
    }
}



