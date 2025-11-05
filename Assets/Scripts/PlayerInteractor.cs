using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : NetworkBehaviour
{
    //[SerializeField] private ObjectPickable currentItem;
    
    [SerializeField] private NetworkObject itemHolder;
    [SerializeField] private float interactionDistance;
    
    private InputSystem_Actions _inputSystem;
    private Camera _camera;
    private PlayerInventoryScript _playerInventory;

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
        _playerInventory = GetComponent<PlayerInventoryScript>();
    }

    private void OnEnable()
    {
        _inputSystem.Enable();
        _inputSystem.Player.Interact.performed += OnInteraction;
        _inputSystem.Player.Drop.performed += OnItemDrop;
    }

    private void OnDisable()
    {
        _inputSystem.Disable();
        _inputSystem.Player.Interact.performed -= OnInteraction;
        _inputSystem.Player.Drop.performed -= OnItemDrop;
    }

    private void OnInteraction(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (!context.performed) return;

        RaycastHit hit;

        if (!Physics.Raycast(_camera.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, interactionDistance)) return;

        if (hit.collider.TryGetComponent<NetworkObject>(out var netObj))
        {
            if (netObj.TryGetComponent<ObjectPickable>(out var pickup))
            {
                Pickup_Server(netObj, itemHolder);
            }
        }
    }

    private void OnItemDrop(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (!context.performed) return;
        
        DropItem_Server();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void Pickup_Server(NetworkObject obj, NetworkObject holder)
    {
        if (obj != null && obj.TryGetComponent<ObjectPickable>(out var pickup))
        {
            if (!_playerInventory.CheckForEmptySlot()) return;
            
            _playerInventory.AddItem(pickup, holder);
            //currentItem = _playerInventory.currentItem.Value;
            //pickup.Pickup(holder);
        }
    }
    
    [ServerRpc(RequireOwnership = true)]
    private void DropItem_Server()
    {
        if (_playerInventory.currentItem.Value == null) return;
        
        //_playerInventory.currentItem.Value.Drop();
        _playerInventory.RemoveItem(_playerInventory.currentItem.Value);
        //currentItem.Drop();
        //currentItem = null;
    }
}



