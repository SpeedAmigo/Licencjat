using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class EnergyRefillScript : NetworkBehaviour, IInteractable
{
    [SerializeField] private string interactText;
    
    [AllowMutableSyncType] private List<IRechargeable> rechargables;
    
    public void Interact(PlayerRoot playerRoot)
    {
        PlayerInventoryScript inventoryScript = playerRoot.gameObject.GetComponent<PlayerInventoryScript>();

        if (inventoryScript == null)
        {
            Debug.Log("Inventory script not found");
            return;
        }
        
        Debug.Log(inventoryScript.currentItem.GetType());

        if (inventoryScript.currentItem.Value is IRechargeable rechargeable)
        {
            rechargeable.Recharge();
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
