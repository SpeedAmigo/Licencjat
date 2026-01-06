using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class PlayerInteractor : NetworkBehaviour
{
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
    
    private bool _primaryHold;
    private bool _secondaryHold;

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
        
        _inputSystem.Player.Primary.performed += OnPrimaryPerformed;
        _inputSystem.Player.Primary.started += OnPrimaryStarted;
        _inputSystem.Player.Primary.canceled += OnPrimaryCanceled;
        
        _inputSystem.Player.Secondary.performed += OnSecondaryPerformed;
        _inputSystem.Player.Secondary.started += OnSecondaryStarted;
        _inputSystem.Player.Secondary.canceled += OnSecondaryCanceled;

        OxygenScript.OnDieEvent += Die;
    }
    
    private void OnDisable()
    {
        _inputSystem.Disable();
        _inputSystem.Player.Interact.performed -= OnInteraction;
        _inputSystem.Player.Drop.performed -= OnItemDrop;
        
        _inputSystem.Player.Primary.performed -= OnPrimaryPerformed;
        _inputSystem.Player.Primary.started -= OnPrimaryStarted;
        _inputSystem.Player.Primary.canceled -= OnPrimaryCanceled;
        
        _inputSystem.Player.Secondary.performed -= OnSecondaryPerformed;
        _inputSystem.Player.Secondary.started -= OnSecondaryStarted;
        _inputSystem.Player.Secondary.canceled -= OnSecondaryCanceled;

        OxygenScript.OnDieEvent -= Die;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (_primaryHold)
        {
            OnPrimaryHold();
        }

        if (_secondaryHold)
        {
            OnSecondaryHold();
        }
    }

    private void OnPrimaryPerformed(InputAction.CallbackContext context)
    {
        if (ItemIsValid() && _playerInventory.currentItem.Value is IPrimaryClick primaryClick)
        {
            primaryClick.OnPrimaryClick();
        }
    }
    
    private void OnSecondaryPerformed(InputAction.CallbackContext context)
    {
        if (ItemIsValid() && _playerInventory.currentItem.Value is ISecondaryClick secondaryClick)
        {
            secondaryClick.OnSecondaryClick();
        }
    }
    
    private void OnPrimaryHold()
    {
        if (ItemIsValid() && _playerInventory.currentItem.Value is IPrimaryHold primaryHold)
        {
            primaryHold.OnPrimaryHold();
        }
    }
    
    private void OnSecondaryHold()
    {
        if (ItemIsValid() && _playerInventory.currentItem.Value is ISecondaryHold secondaryHold)
        {
            secondaryHold.OnSecondaryHold();
        }
    }

    private void OnPrimaryStarted(InputAction.CallbackContext context)
    {
        _primaryHold = true;
    }

    private void OnPrimaryCanceled(InputAction.CallbackContext context)
    {
        _primaryHold = false;
        
        if (_playerInventory.currentItem.Value is IPrimaryCancel primaryCancel)
        {
            primaryCancel.OnPrimaryCancel();
        }
    }

    private void OnSecondaryStarted(InputAction.CallbackContext context)
    {
        _secondaryHold = true;
    }

    private void OnSecondaryCanceled(InputAction.CallbackContext context)
    {
        _secondaryHold = false;
        
        if (_playerInventory.currentItem.Value is ISecondaryCancel secondaryCancel)
        {
            secondaryCancel.OnSecondaryCancel();
        }
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
            else if (netObj.TryGetComponent<IInteractable>(out var interactable))
            {
                Interact_Server(netObj);
            }
        }
    }

    private void OnItemDrop(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (!context.performed) return;
        
        DropItem_Server();
    }

    
    // to get rid of so much component checking
    // try to write network serializer because otherwise it won't work
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
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void Interact_Server(NetworkObject netObj)
    {
        Interact_Client(netObj);
    }

    [ObserversRpc]
    private void Interact_Client(NetworkObject netObj)
    {
        if (netObj.TryGetComponent<IInteractable>(out var interactable))
        {
            interactable.Interact();
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
        }
    }
    
    private bool ItemIsValid()
    {
        if (_playerInventory == null)
        {
            Debug.Log("No player inventory");
            return false;
        }

        if (_playerInventory.currentItem.Value == null)
        {
            //Debug.Log("No item in hand"); 
            return false;
        }
        
        return true;
    }
    
    private void Die()
    {
        enabled = false;
    }
}



