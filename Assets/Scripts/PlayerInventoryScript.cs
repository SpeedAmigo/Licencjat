using System;
using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
        }
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
        UpdateDrawItem(index);
    }

    [ObserversRpc(BufferLast = true)]
    private void UpdateDrawItem(int index)
    {
        currentItemIndex = index;
        
        if (slots[index] != null)
        {
            // disable current item if exists
            if (currentItem.Value != null)
            {
                currentItem.Value.gameObject.SetActive(false);
            }
            
            // enable new item and set it as current
            currentItem.Value = slots[index];
            currentItem.Value.gameObject.SetActive(true);
        }
        else if (slots[index] == null && currentItem.Value != null)
        {
            // disable current item if exists
            currentItem.Value.gameObject.SetActive(false);
            currentItem.Value = null;
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

    public void AddItem(ObjectPickable item)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                
                UpdateUIAdd(Owner, i, item); // update UI with free slot index and icon
                AddItem_Client(i, item); // show or hide item for clients
                break;
            }
        }
    }

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
    private void AddItem_Client(int index, ObjectPickable item)
    {
        // disable item if not current on current index
        if (index != currentItemIndex)
        {
            item.gameObject.SetActive(false);   
        }
        else
        {
            item.gameObject.SetActive(true);
            currentItem.Value = item;
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
