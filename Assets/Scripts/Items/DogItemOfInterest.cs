using System;
using FishNet.Object;
using Items;
using UnityEngine;

public class DogItemOfInterest : Item
{
    public event Action ItemPickedUp;
    public event Action ItemDropped;

    protected override void PickupLogic(NetworkObject holder)
    {
        base.PickupLogic(holder);
        ItemPickedUp?.Invoke();
    }

    protected override void DropLogic()
    {
        base.DropLogic();
        ItemDropped?.Invoke();
    }
}
