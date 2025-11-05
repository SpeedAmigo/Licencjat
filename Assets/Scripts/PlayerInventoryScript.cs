using System;
using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Heathen.SteamworksIntegration.API;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventoryScript : NetworkBehaviour
{
    [AllowMutableSyncType] public SyncVar<ObjectPickable> currentItem = new();
    
    public static event Action<int, Sprite> OnUIUpdateAdd;
    public static event Action<int> OnUIUpdateRemove;
    
    [SerializeField] private int inventorySize = 4;
    [SerializeField] private int currentItemIndex;
    
    [SerializeField, AllowMutableSyncType] private SyncList<ObjectPickable> slots = new();
    
    private InputSystem_Actions _inputSystem;

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
    
    private void AddInventorySlots()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            slots.Add(null);
        }
    }

    private void Awake()
    {
        _inputSystem = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputSystem.Enable();
        _inputSystem.Player.Slot1 .performed += OnSlot1;
        _inputSystem.Player.Slot2 .performed += OnSlot2;
        _inputSystem.Player.Slot3 .performed += OnSlot3;
    }

    private void OnDisable()
    {
        _inputSystem.Disable();
        _inputSystem.Player.Slot1 .performed -= OnSlot1;
        _inputSystem.Player.Slot2 .performed -= OnSlot2;
        _inputSystem.Player.Slot3 .performed -= OnSlot3;
    }
    
    private void OnSlot1(InputAction.CallbackContext ctx) => OnDrawCurrentItem(0);
    private void OnSlot2(InputAction.CallbackContext ctx) => OnDrawCurrentItem(1);
    private void OnSlot3(InputAction.CallbackContext ctx) => OnDrawCurrentItem(2);

    private void OnDrawCurrentItem(int index)
    {
        OnDrawCurrentItem_Server(index);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnDrawCurrentItem_Server(int index)
    {
        if (index < 0 || index >= slots.Count) return;
        
        currentItemIndex = index;
        currentItem.Value = slots[index];
    }
    
    private void HandleCurrentItemChange(ObjectPickable prev, ObjectPickable next, bool asServer)
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
    public void AddItem(ObjectPickable item, NetworkObject holder)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                
                UpdateUIAdd(Owner, i, item); // update UI with free slot index and icon
                item.Pickup(holder);

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
    public void RemoveItem(ObjectPickable item)
    {
        if (slots.Contains(item))
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == item)
                {
                    slots[i] = null;
                    UpdateUIRemove(Owner, i); // update UI with free slot index and null icon
                    item.Drop();
                    
                    if (currentItem.Value == item)
                    {
                        currentItem.Value = null;
                    }
                    break;
                }
            }
        }
    }
    
    [Server]
    public void RequestRemoveItem(ObjectPickable item, PlayerInventoryScript inventory)
    {
        if (inventory == null || item == null) return;
        
        inventory.RemoveItem(item);
    }

    [ObserversRpc(BufferLast = true)]
    private void SetItem_Client(ObjectPickable item, bool active)
    {
        if (item != null && item.gameObject != null)
        {
            item.gameObject.SetActive(active);
        }
    }

    [TargetRpc]
    private void UpdateUIRemove(NetworkConnection conn, int index)
    {
        OnUIUpdateRemove?.Invoke(index);
    }

    [TargetRpc]
    private void UpdateUIAdd(NetworkConnection conn, int index, ObjectPickable item)
    {
        OnUIUpdateAdd?.Invoke(index, item.itemIcon); // passing free slot index and icon
    }
}
