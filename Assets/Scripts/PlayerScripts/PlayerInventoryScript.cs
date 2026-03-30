using System;
using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class PlayerInventoryScript : PlayerComponent
{
    public static event Action<int, Sprite> OnUIUpdateAdd;
    public static event Action<int> OnUIUpdateRemove;
    public static event Action<int> OnUIFrameUpdate;
    
    [Header("Hand Rigs")]
    [GUIColor("Red")]
    [SerializeField] private GameObject rightHandRigs;
    
    [GUIColor("Blue")]
    [AllowMutableSyncType] public SyncVar<Item> currentItem = new();
    [GUIColor("Blue")]
    [SerializeField] private int currentItemIndex;
    
    [GUIColor("Yellow")]
    [SerializeField] private int inventorySize = 4;
    
    [AllowMutableSyncType] public SyncList<Item> slots = new();
    
    private InputSystem_Actions _inputSystem;

    #region GeneralMethods 
    public override void OnStartServer()        
    {                                   
        base.OnStartServer();
        AddInventorySlots();
        currentItem.OnChange += HandleCurrentItemChange;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        currentItem.OnChange -= HandleCurrentItemChange;
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        currentItem.OnChange += HandleCurrentItemChange;
        
        OnUIFrameUpdate?.Invoke(currentItemIndex);
        
        if (!IsOwner)
        {
            enabled = false;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        currentItem.OnChange -= HandleCurrentItemChange;
    }
    
    protected override void Awake()
    {
        base.Awake();
        
        _inputSystem = new InputSystem_Actions();
    }

    protected override void OnEnable()
    {
        _inputSystem.Enable();
        _inputSystem.Player.Slot1 .performed += OnSlot1;
        _inputSystem.Player.Slot2 .performed += OnSlot2;
        _inputSystem.Player.Slot3 .performed += OnSlot3;
    }

    protected override void OnDisable()
    {
        _inputSystem.Disable();
        _inputSystem.Player.Slot1 .performed -= OnSlot1;
        _inputSystem.Player.Slot2 .performed -= OnSlot2;
        _inputSystem.Player.Slot3 .performed -= OnSlot3;
    }
    #endregion
    
    #region InputBinding
    private void OnSlot1(InputAction.CallbackContext ctx) => OnDrawCurrentItem(0);
    private void OnSlot2(InputAction.CallbackContext ctx) => OnDrawCurrentItem(1);
    private void OnSlot3(InputAction.CallbackContext ctx) => OnDrawCurrentItem(2);
    #endregion
    
    #region Helpers
    
    private void HandleCurrentItemChange(Item prev, Item next, bool asServer)
    {
        if (next != null && next.gameObject != null && !next.gameObject.activeSelf)
        {
            next.gameObject.SetActive(true);
        }
        
        if (prev != null && prev != next && prev.gameObject != null && prev.gameObject.activeSelf)
        {
            if (slots.Contains(prev))
            {
                prev.gameObject.SetActive(false);   
            }
        }
        
        float targetWeight = next != null ? 1f : 0f;
        RigWeightHandler(rightHandRigs, targetWeight);
    }
    
    private void OnDrawCurrentItem(int index)
    {
        if (!playerRoot.isAlive.Value) return;
        
        if (currentItem.Value != null && currentItem.Value.isBig) return;
        
        OnUIFrameUpdate?.Invoke(index);
        
        OnDrawCurrentItem_Server(index);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnDrawCurrentItem_Server(int index)
    {
        if (index < 0 || index >= slots.Count) return;
        Item slotItem = slots[index];
        
        //hiding item if pressed the same button
        if (currentItemIndex == index)
        {
            if (currentItem.Value != null)
            {
                currentItem.Value.gameObject.SetActive(false);
                currentItem.Value = null;
                return;
            }
            else
            {
                currentItemIndex = index;
                currentItem.Value = slotItem;
                return;
            }
        }
        
        // if pressed different key than the current slot index
        currentItemIndex = index;
        currentItem.Value = slots[index];
    }
    
    public bool CheckForEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (slot == null)
            {
                return true;
            }
        }
        return false;
    }
    
    [Server]
    public void RequestRemoveItem(Item item, PlayerInventoryScript inventory)
    {
        if (inventory == null || item == null) return;

        if (item.isBig)
        {
            inventory.RemoveBigItem(item, Vector3.forward);
        }
        else
        {
            inventory.RemoveItem(item, Vector3.forward);
        }
    }
    
    /*[Server]
    public void RequestRemoveItem(Item item, PlayerInventoryScript inventory, bool setActiveOnDrop)
    {
        if (inventory == null || item == null) return;

        item.gameObject.SetActive(setActiveOnDrop);
        
        if (item.isBig)
        {
            inventory.RemoveBigItem(item, Vector3.forward);
        }
        else
        {
            inventory.RemoveItem(item, Vector3.forward);
        }
    }*/
    
    private void AddInventorySlots()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            slots.Add(null);
        }
    }
    
    #endregion

    #region BigItem
    
    [Server]
    public void AddBigItem(Item bigItem, NetworkObject fpHolder, NetworkObject tpHolder, NetworkConnection conn)
    {
        if (currentItem.Value == null)
        {
            bigItem.Pickup(fpHolder, tpHolder, conn);
            currentItem.Value = bigItem;
        }
    }

    [Server]
    public void RemoveBigItem(ObjectPickable bigItem, Vector3 rotation)
    {
        if (currentItem.Value && bigItem.isBig)
        {
            bigItem.Drop(rotation);
            currentItem.Value = null;
        }
    }
    
    #endregion

    #region RegularItem
    
    [Server]
    public void AddItem(Item item, NetworkObject fpHolder, NetworkObject tpHolder, NetworkConnection conn)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                
                UpdateUIAdd(Owner, i, item); // update UI with free slot index and icon
                item.Pickup(fpHolder, tpHolder, conn);
                
                if (i == currentItemIndex)
                {
                    currentItem.Value = item;
                }
                else
                {
                    if (item != null && item.gameObject != null)
                    {
                        item.gameObject.SetActive(false);
                    }
                    SetItem_Client(item, false);
                }
                break;
            }
        }
    }

    [Server]
    public void RemoveItem(Item item, Vector3 rotation)
    {
        if (slots.Contains(item))
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == item)
                {
                    slots[i] = null;
                    UpdateUIRemove(Owner, i); // update UI with free slot index and null icon
                    
                    item.Drop(rotation);
                    
                    if (currentItem.Value == item)
                    {
                        currentItem.Value = null;
                    }
                    break;
                }
            }
        }
    }
    
    [ObserversRpc(BufferLast = true)]
    private void SetItem_Client(ObjectPickable item, bool active)
    {
        if (item != null && item.gameObject != null)
        {
            item.gameObject.SetActive(active);
        }
    }
    
    #endregion
    
    #region UI
    
    [TargetRpc]
    private void UpdateUIRemove(NetworkConnection conn, int index)
    {
        OnUIUpdateRemove?.Invoke(index);
    }

    [TargetRpc]
    private void UpdateUIAdd(NetworkConnection conn, int index, Item item)
    {
        OnUIUpdateAdd?.Invoke(index, item.itemIcon); // passing free slot index and icon
    }
    
    #endregion
    
    #region RigWeight
    
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
    
    #endregion
}
