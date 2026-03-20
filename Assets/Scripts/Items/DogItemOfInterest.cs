using System;
using FishNet.Object;
using Items;
using UnityEngine;

public class DogItemOfInterest : Item
{
    public event Action<NetworkObject> HoldingPlayer;
    
    public event Action ItemPickedUp;
    public event Action ItemDropped;

    protected override void PickupLogic(NetworkObject holder)
    {
        base.PickupLogic(holder);
        ItemPickedUp?.Invoke();
        HoldingPlayer?.Invoke(holder);
    }

    protected override void DropLogic(Vector3 forward)
    {
        base.DropLogic(forward);
        ItemDropped?.Invoke();
    }
}
