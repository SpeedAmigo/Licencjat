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
        if (!context.performed || !IsOwner) return;

        RaycastHit hit;

        if (!Physics.Raycast(_camera.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, interactionDistance)) return;

        if (hit.collider.TryGetComponent<NetworkObject>(out var netObj))
        {
            if (hit.collider.TryGetComponent<IPickable>(out var pickable))
            {
                Pickup_Server(netObj, itemHolder);
            }
            else if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                Interact_Server(netObj);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void Pickup_Server(NetworkObject obj, NetworkObject holder, NetworkConnection picker = null)
    {
        if (obj.TryGetComponent<IPickable>(out var pickable))
        {
            pickable.Pickup(picker, holder);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void Interact_Server(NetworkObject obj)
    {
        if(obj.TryGetComponent<IInteractable>(out var interactable))
        {
            interactable.Interact();
        }
    }
}
