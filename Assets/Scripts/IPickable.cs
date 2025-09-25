using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public interface IPickable
{
    public void Pickup(NetworkObject holder);
}
