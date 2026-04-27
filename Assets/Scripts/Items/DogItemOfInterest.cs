using System;
using FishNet.Connection;
using FishNet.Object;
using Items;
using UnityEngine;

public class DogItemOfInterest : Item
{
    public event Action<NetworkObject> HoldingPlayer;
    
    public event Action ItemPickedUp;
    public event Action ItemDropped;
    
    protected override void PickupLogic(NetworkObject holder, NetworkConnection conn)
    {
        base.PickupLogic(holder, conn);
        ItemPickedUp?.Invoke();
        HoldingPlayer?.Invoke(holder);
    }

    protected override void DropLogic(Vector3 position, Vector3 forward)
    {
        base.DropLogic(position, forward);
        ItemDropped?.Invoke();
    }
}
