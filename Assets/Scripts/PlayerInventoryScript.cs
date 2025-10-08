using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayerInventoryScript : NetworkBehaviour
{
    public static event Action<int, Sprite> OnUIUpdate;
    
    [SerializeField] private int inventorySize = 4;
    
    public List<GameObject> slots = new();
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
        }
        
        for (int i = 0; i < inventorySize; i++)
        {
            slots.Add(null);
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
                slots[i] = item.gameObject;
                OnUIUpdate?.Invoke(i, item.itemIcon); // passing free slot index and icon
                break;
            }
        }
    }
}
