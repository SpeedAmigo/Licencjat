using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FMODUnity;
using UnityEngine;

public class EnergyRefillScript : BaseInteractable
{
    [SerializeField] private Transform soundPosition;
    [SerializeField] private EventReference rechargeSound;
    
    [AllowMutableSyncType] private List<IRechargeable> rechargables;
    
    public override void Interact(PlayerRoot playerRoot)
    {
        if (playerRoot == null) return;
        
        PlayerInventoryScript inventoryScript = playerRoot.gameObject.GetComponent<PlayerInventoryScript>();

        if (inventoryScript == null)
        {
            Debug.Log("Inventory script not found");
            return;
        }
        
        if (inventoryScript.currentItem.Value is IRechargeable rechargeable)
        {
            rechargeable.Recharge();
            SoundCreator.Instance.PlayOneShot(rechargeSound, soundPosition.position);
        }
    }
}