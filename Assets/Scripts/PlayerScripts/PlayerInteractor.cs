using System;
using FishNet.Connection;
using FishNet.Object;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;

public class PlayerInteractor : PlayerComponent
{
    public static event Action<string> OnObjectDetection; 
    public static event Action OnObjectUnDetection; 
    
    [Header("Item Holders")]
    [GUIColor("Red")]
    [SerializeField] private NetworkObject fpItemHolder;
    [GUIColor("Red")]
    [SerializeField] private NetworkObject tpIemHolder;

    [Header("Drop Settings")]
    [HideInInspector] public NetworkObject itemDropTransform;
    
    [SerializeField] private float dropRadius = 0.25f;
    [SerializeField] private float dropDistance = 1f;
    [SerializeField] private float lookDownThreshold = 0.8f;
    
    [Header("Cross indicator")]
    [SerializeField] private GameObject crossIndicator;
    
    [Header("Interaction Distance Settings")]
    [GUIColor("Yellow")]
    [SerializeField] private float interactionDistance;
    [SerializeField] private Color interactionColor;
    [SerializeField] private Color defaultColor;
    
    private InputSystem_Actions _inputSystem;
    private Camera _camera;
    private PlayerInventoryScript _playerInventory;
    
    private bool _primaryHold;
    private bool _secondaryHold;

    private RaycastHit _hit;
    private bool _hasValidTarget;
    
    private IOutlineChangeable _currentOutlineChangeable;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        
        _inputSystem = new InputSystem_Actions();
        _camera = Camera.main;
        _playerInventory = GetComponent<PlayerInventoryScript>();
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        _inputSystem.Enable();
        _inputSystem.Player.Interact.performed += OnInteraction;
        _inputSystem.Player.Drop.performed += OnItemDrop;
        
        _inputSystem.Player.Primary.performed += OnPrimaryPerformed;
        _inputSystem.Player.Primary.started += OnPrimaryStarted;
        _inputSystem.Player.Primary.canceled += OnPrimaryCanceled;
        
        _inputSystem.Player.Secondary.performed += OnSecondaryPerformed;
        _inputSystem.Player.Secondary.started += OnSecondaryStarted;
        _inputSystem.Player.Secondary.canceled += OnSecondaryCanceled;
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        
        _inputSystem.Disable();
        _inputSystem.Player.Interact.performed -= OnInteraction;
        _inputSystem.Player.Drop.performed -= OnItemDrop;
        
        _inputSystem.Player.Primary.performed -= OnPrimaryPerformed;
        _inputSystem.Player.Primary.started -= OnPrimaryStarted;
        _inputSystem.Player.Primary.canceled -= OnPrimaryCanceled;
        
        _inputSystem.Player.Secondary.performed -= OnSecondaryPerformed;
        _inputSystem.Player.Secondary.started -= OnSecondaryStarted;
        _inputSystem.Player.Secondary.canceled -= OnSecondaryCanceled;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!playerRoot.isAlive.Value) return;
        
        if (_primaryHold)
        {
            OnPrimaryHold();
        }

        if (_secondaryHold)
        {
            OnSecondaryHold();
        }
        
        DetectTarget();
    }
    
    private void DetectTarget()
    {
        var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        bool found = false;
        string interactText = null;
        
        IOutlineChangeable newOutlineChangeable = null;

        if (Physics.Raycast(ray, out _hit, interactionDistance))
        {
            var collider = _hit.collider;

            if (collider.TryGetComponent(out Item pickable))
            {
                if (pickable != _playerInventory.currentItem.Value)
                {
                    interactText = pickable.itemDisplayName;
                    found = true;
                }

                if (pickable is IOutlineChangeable outlineChangeable)
                {
                    newOutlineChangeable = outlineChangeable;
                }
            }
            else if (collider.TryGetComponent(out IInteractable interactable))
            {
                interactText = interactable.GetInteractText();

                if (interactable is IOutlineChangeable outlineChangeable)
                {
                    newOutlineChangeable = outlineChangeable;
                }
                
                found = true;
            }
        }

        if (_currentOutlineChangeable != null && _currentOutlineChangeable != newOutlineChangeable)
        {
            _currentOutlineChangeable.SetOutlineColor(defaultColor);
        }

        if (newOutlineChangeable != null)
        {
            newOutlineChangeable.SetOutlineColor(interactionColor);
        }
        
        _currentOutlineChangeable = newOutlineChangeable;
        
        if (found)
        {
            OnObjectDetection?.Invoke(interactText);
        }
        else if (_hasValidTarget)
        {
            OnObjectUnDetection?.Invoke();
        }

        _hasValidTarget = found;
    }

    #region  Mouse Left/Right actions
        
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
    
    #endregion
    
    private void OnInteraction(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (!context.performed) return;
        if (!playerRoot.isAlive.Value) return;

        if (!Physics.Raycast(_camera.ScreenPointToRay(Mouse.current.position.ReadValue()), out var hit, interactionDistance)) return;
        
        if (!hit.collider.TryGetComponent<NetworkObject>(out var netObj)) return;
        
        if (netObj.TryGetComponent<Item>(out var pickup))
        {
            if (pickup == _playerInventory.currentItem.Value) return;
            Pickup_Server(netObj, fpItemHolder, tpIemHolder, Owner);
        }
        else if (netObj.TryGetComponent<IInteractable>(out var interactable))
        {
            Interact_Server(netObj);
        }
    }

    private void OnItemDrop(InputAction.CallbackContext context)
    {
        if (!IsOwner || !context.performed || !GlobalDropRule.CanDropItems) return;

        if (IsLookingTooFarDown())
        {
            MessageShowerScript.Instance.ShowMessage("Can't drop looking too far down", 1f);
            return;
        }

        if (!TryGetDropPosition(out Vector3 dropPos))
        {
            MessageShowerScript.Instance.ShowMessage("Too close to obstacle", 1f);
            return;
        }
        
        Vector3 direction = transform.forward;
        
        DropItem_Server(dropPos, direction);
    }
    
    public bool TryGetDropPosition(out Vector3 dropPosition)
    {
        Vector3 origin = itemDropTransform.transform.position;
        Vector3 direction = itemDropTransform.transform.forward;

        float minAllowedDistance = dropDistance * 1;

        int mask = ~LayerMask.GetMask("PickableLayer");

        if (Physics.SphereCast(origin, dropRadius, direction, out RaycastHit hit, dropDistance, mask))
        {
            Debug.DrawLine(origin, hit.point, Color.red, 2f);
            
            if (hit.distance < minAllowedDistance)
            {
                Instantiate(crossIndicator, hit.point, Quaternion.LookRotation(hit.normal));
                dropPosition = Vector3.zero;
                return false;
            }
            
            dropPosition = hit.point - direction * dropRadius;
            return true;
        }
        
        dropPosition = origin + direction * dropDistance;
        return true;
    }

    private bool IsLookingTooFarDown()
    {
        Vector3 forward = itemDropTransform.transform.forward;
        
        float dot = Vector3.Dot(forward, Vector3.down);

        return dot > lookDownThreshold;
    }
    
    // to get rid of so much component checking
    // try to write network serializer because otherwise it won't work
    [ServerRpc(RequireOwnership = false)]
    private void Pickup_Server(NetworkObject obj, NetworkObject fpHolder, NetworkObject tpHolder, NetworkConnection conn)
    {
        if (obj != null && obj.TryGetComponent<Item>(out var pickup))
        {
            if (pickup.isBig)
            {
                _playerInventory.AddBigItem(pickup, fpHolder, tpHolder, conn);
            }
            else if (_playerInventory.CheckForEmptySlot() && !pickup.isBig)
            {
                _playerInventory.AddItem(pickup, fpHolder, tpHolder, conn);
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
            interactable.Interact(playerRoot);
        }
    }
    
    [ServerRpc(RequireOwnership = true)]
    private void DropItem_Server(Vector3 position, Vector3 rotation)
    {
        if (_playerInventory.currentItem.Value == null) return;

        if (_playerInventory.currentItem.Value.isBig)
        {
            _playerInventory.RemoveBigItem(_playerInventory.currentItem.Value, position, rotation);
        }
        else
        {
            _playerInventory.RemoveItem(_playerInventory.currentItem.Value, position, rotation);
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
    
    private void OnDrawGizmos()
    {
        if (itemDropTransform == null) return;

        Vector3 origin = itemDropTransform.transform.position;
        Vector3 direction = itemDropTransform.transform.forward;

        // Start sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, dropRadius);

        // End sphere
        Vector3 end = origin + direction * dropDistance;
        Gizmos.DrawWireSphere(end, dropRadius);

        // Line between them
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, end);
    }
}



