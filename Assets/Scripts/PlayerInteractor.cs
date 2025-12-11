using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class PlayerInteractor : NetworkBehaviour
{
    [Header("Hand Rigs")]
    [GUIColor("Red")]
    [SerializeField] private GameObject rightHandRigs;
    
    [Header("Item Holders")]
    [GUIColor("Red")]
    [SerializeField] private NetworkObject fpItemHolder;
    [GUIColor("Red")]
    [SerializeField] private NetworkObject tpIemHolder;
    
    [Header("Interaction Distance Settings")]
    [GUIColor("Yellow")]
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
                Pickup_Server(netObj, fpItemHolder, tpIemHolder);
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
    private void Pickup_Server(NetworkObject obj, NetworkObject fpHolder, NetworkObject tpHolder)
    {
        if (obj != null && obj.TryGetComponent<ObjectPickable>(out var pickup))
        {
            if (pickup.isBig)
            {
                _playerInventory.AddBigItem(pickup, fpHolder, tpHolder);
            }
            else if (_playerInventory.CheckForEmptySlot() && !pickup.isBig)
            {
                _playerInventory.AddItem(pickup, fpHolder, tpHolder);
                RigWeightHandler(rightHandRigs, 1f);
            }
            
            //if (!_playerInventory.CheckForEmptySlot()) return;
            //_playerInventory.AddItem(pickup, holder);
            
            //currentItem = _playerInventory.currentItem.Value;
            //pickup.Pickup(holder);
        }
    }
    
    [ServerRpc(RequireOwnership = true)]
    private void DropItem_Server()
    {
        if (_playerInventory.currentItem.Value == null) return;

        if (_playerInventory.currentItem.Value.isBig)
        {
            _playerInventory.RemoveBigItem(_playerInventory.currentItem.Value);
        }
        else
        {
            _playerInventory.RemoveItem(_playerInventory.currentItem.Value);
            RigWeightHandler(rightHandRigs, 0f);
        }
        
        //_playerInventory.currentItem.Value.Drop();
        
        //currentItem.Drop();
        //currentItem = null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RigWeightHandler(GameObject rigHolder, float weight)
    {
        rigHolder.GetComponent<Rig>().weight = weight;
        
        RigWeightHandlerClient(rigHolder, weight);
    }

    [ObserversRpc]
    private void RigWeightHandlerClient(GameObject rigHolder, float weight)
    {
        rigHolder.GetComponent<Rig>().weight = weight;
    }
}



